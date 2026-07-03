using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using InternMS.Infrastructure.Data;
using InternMS.Domain.Entities;
using InternMS.Api.Services.Email;
using InternMS.Api.Services.Token;
using InternMS.Api.DTOs.Authentication; 
using System;
using Microsoft.AspNetCore.Http.HttpResults;
using InternMS.Api.DTOs.Users;


namespace InternMS.Api.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService>? _logger;

        public AuthService(AppDbContext db, IEmailService emailService, ITokenService tokenService, ILogger<AuthService>? logger = null)
        {
            _db = db;
            _emailService = emailService;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<User?> ConfirmEmailAsync(string token)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u=>u.EmailConfirmationToken == token);
            if(user == null || user.EmailConfirmationTokenExpires < DateTime.UtcNow)
            {
                return null;
            }
            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpires = null;
            if (user.AdminApproved)
            {
                user.IsActive = true;
            }
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ApproveUserAsync(Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u=>u.Id == userId);
            if(user == null)
            {
                return false;
            }
            user.AdminApproved = true;
            if (user.EmailConfirmed)
            {
                user.IsActive = true;
            }
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<User?> ValidateCredentialsAsync(string email, string password)
        {
            var user = await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if(user == null) return null;

            if(!user.EmailConfirmed || !user.AdminApproved)
            {
                throw new Exception("Account not verified.");
            }

            bool Verified = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return Verified? user : null ;
        }

        public async Task<User?> RegisterUserAsync(string email, string password, string firstName, string lastName, int roleId)
        {
            if(await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
            {
                throw new InvalidOperationException("Email already in use");
            }

            var token = Guid.NewGuid().ToString();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmationToken = token,
                EmailConfirmationTokenExpires = DateTime.UtcNow.AddHours(24),
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = roleId
            };
            _db.UserRoles.Add(userRole);

            await _db.SaveChangesAsync();

            var confirmLink = $"http://localhost:5248/api/auth/confirm-email?token={token}";
            var verifyUserLink = $"http://localhost:5248/api/auth/approve-user?userId={user.Id}";
            
            await _emailService.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your email by clicking <a href='{confirmLink}'>here</a>. This link will expire in 24 hours."
            );

            await _emailService.SendEmailAsync(
                "2021uec1535@mnit.ac.in",
                "New User Registration",
                $"A new user has registered with the email: {email}. Please review and approve the account by clicking <a href='{verifyUserLink}'>here</a>."
            );

            await _db.Entry(user).Collection(u => u.UserRoles).LoadAsync();

            return user;
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var token = await _db.RefreshTokens.FirstOrDefaultAsync(t=>t.Token == refreshToken);

            if(token != null)
            {
                token.IsRevoked = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task LogoutAsync(Guid userId)
        {
            // Revoke all refresh tokens for this user
            var userTokens = await _db.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }

            if (userTokens.Any())
            {
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            return await _db.BlacklistedTokens.AnyAsync(t=> t.Token == token && t.ExpiryDate > DateTime.UtcNow);
        }

        public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _db.RefreshTokens
                .Include(r => r.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

            if (storedToken == null)
            {
                Console.WriteLine($"[TOKEN_REFRESH] ❌ Refresh token NOT found or already revoked at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                return null;
            }

            Console.WriteLine($"[TOKEN_REFRESH] ✅ Valid refresh token found for user: {storedToken.User.Email}");

            var user = storedToken.User;
            var roles = user.UserRoles?.Select(r => r.Role.Name) ?? new string[0];

            // Generate new access token
            var accessToken = _tokenService.CreateAccessToken(
                user.Id, 
                user.Email, 
                user.FirstName, 
                user.LastName, 
                roles
            );

            // Generate new refresh token
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            
            // Revoke old refresh token
            storedToken.IsRevoked = true;
            _db.RefreshTokens.Update(storedToken);

            // Add new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = refreshTokenExpiresAt
            };
            _db.RefreshTokens.Add(newRefreshTokenEntity);

            await _db.SaveChangesAsync();

            // Calculate expiration timestamps (Unix time in seconds)
            var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpirationMinutes());
            var accessTokenUnixTime = new DateTimeOffset(accessTokenExpiresAt).ToUnixTimeSeconds();
            var refreshTokenUnixTime = new DateTimeOffset(refreshTokenExpiresAt).ToUnixTimeSeconds();

            var accessTokenExpirationMinutes = _tokenService.GetAccessTokenExpirationMinutes();
            Console.WriteLine($"[TOKEN_REFRESH] 🔄 Generating new tokens:");
            Console.WriteLine($"[TOKEN_REFRESH]   - Access Token: expires in {accessTokenExpirationMinutes} minutes (at {accessTokenExpiresAt:yyyy-MM-dd HH:mm:ss} UTC)");
            Console.WriteLine($"[TOKEN_REFRESH]   - Refresh Token: expires in 7 days (at {refreshTokenExpiresAt:yyyy-MM-dd HH:mm:ss} UTC)");
            Console.WriteLine($"[TOKEN_REFRESH]   - Next refresh should occur in {accessTokenExpirationMinutes - 1} minutes");

            // Map user to UserDto
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                User = userDto,
                AccessTokenExpiresIn = accessTokenUnixTime,
                RefreshTokenExpiresIn = refreshTokenUnixTime
            };
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                _logger?.LogInformation($"[ForgotPassword] Processing forgot password request for email: {email}");
                
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    // Don't reveal if email exists for security reasons
                    _logger?.LogWarning($"[ForgotPassword] Email not found: {email}");
                    return true;
                }

                _logger?.LogInformation($"[ForgotPassword] User found: {user.Email}");

                var resetToken = Guid.NewGuid().ToString();
                user.PasswordResetToken = resetToken;
                user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

                await _db.SaveChangesAsync();
                _logger?.LogInformation($"[ForgotPassword] Reset token saved for user: {user.Email}");

                // Send password reset email
                var resetLink = $"http://localhost:5173/reset-password?token={resetToken}";
                var emailBody = $"<p>You requested to reset your password. Click <a href='{resetLink}'>here</a> to reset it.</p>" +
                               $"<p>This link will expire in 1 hour.</p>" +
                               $"<p>If you didn't request this, you can safely ignore this email.</p>";

                await _emailService.SendEmailAsync(
                    email,
                    "Reset your password",
                    emailBody
                );

                _logger?.LogInformation($"[ForgotPassword] Password reset email sent successfully to: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[ForgotPassword] Error: {ex.Message}");
                _logger?.LogError($"[ForgotPassword] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<User?> ValidatePasswordResetTokenAsync(string token)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token);
            if (user == null || user.PasswordResetTokenExpires < DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await ValidatePasswordResetTokenAsync(token);
            if (user == null)
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _db.SaveChangesAsync();
            return true;
        }

    }
}