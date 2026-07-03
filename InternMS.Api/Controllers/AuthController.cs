using Microsoft.AspNetCore.Mvc;
using InternMS.Api.DTOs.Authentication;
using InternMS.Api.DTOs.Users;
using InternMS.Api.Services.Auth;
using InternMS.Api.Services.Token;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using InternMS.Api.Middleware;
using InternMS.Domain.Entities;
using InternMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternMS.Api.Controllers
{
    [Route("api/Auth")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;


        public AuthController(IAuthService authService, ITokenService tokenService, IMapper mapper, AppDbContext context, ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _mapper = mapper;
            _context = context;
            Logger = logger;
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmpty(token, "Confirmation token");
                if (!isValid) return err;

                var user = await _authService.ConfirmEmailAsync(token);

                var (exists, badRequest) = ValidateNotNull(user, "User or token");
                if (!exists) return badRequest;

                Logger?.LogInformation("Email confirmed successfully");
                return Ok(new { message = "Email confirmed successfully. You can now log in." });
            }, "ConfirmEmail");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("approve-user")]
        public async Task<IActionResult> ApproveUser(Guid userId)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmptyGuid(userId, "User ID");
                if (!isValid) return err;

                var result = await _authService.ApproveUserAsync(userId);

                if (!result)
                    return BadRequest("User not found or already approved");

                Logger?.LogInformation($"User {userId} approved");
                return Ok(new { message = "User approved successfully." });
            }, "ApproveUser");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto request)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotNull(request, "Registration data");
                if (!isValid) return err;

                var (isEmailValid, emailErr) = ValidateNotEmpty(request.Email, "Email");
                if (!isEmailValid) return emailErr;

                var (isPasswordValid, passwordErr) = ValidateNotEmpty(request.Password, "Password");
                if (!isPasswordValid) return passwordErr;

                var user = await _authService.RegisterUserAsync(request.Email, request.Password, request.FirstName, request.LastName, request.RoleId);

                var (exists, notFound) = ValidateNotNull(user, "User");
                if (!exists) return notFound;

                var userDto = _mapper.Map<UserDto>(user);
                Logger?.LogInformation($"User registered: {request.Email}");
                return Ok(userDto);
            }, "Register");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotNull(request, "Login credentials");
                if (!isValid) return err;

                var user = await _authService.ValidateCredentialsAsync(request.Email, request.Password);

                var (exists, unauthorized) = ValidateNotNull(user, "User account");
                if (!exists) return unauthorized;

                // Check if email is confirmed
                if (!user.EmailConfirmed)
                {
                    return BadRequest(new { 
                        message = "Account not verified",
                        details = "Please verify your email address before logging in. Check your email for the verification link."
                    });
                }

                // Check if admin has approved the account
                if (!user.AdminApproved)
                {
                    return BadRequest(new { 
                        message = "Account pending approval",
                        details = "Your account is pending approval from an administrator. Please wait for approval to log in."
                    });
                }

                // Check if account is active
                if (!user.IsActive)
                {
                    return BadRequest(new { 
                        message = "Account inactive",
                        details = "Your account has been deactivated. Please contact support for assistance."
                    });
                }

                var roles = user.UserRoles?.Select(ur => ur.Role.Name) ?? new string[0];
                var accessToken = _tokenService.CreateAccessToken(user.Id, user.Email, user.FirstName, user.LastName, roles);
                var refreshTokenString = _tokenService.GenerateRefreshToken();
                var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

                // Check if a refresh token already exists for this user
                var existingRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == user.Id);

                if (existingRefreshToken != null)
                {
                    // Update existing refresh token
                    existingRefreshToken.Token = refreshTokenString;
                    existingRefreshToken.ExpiresAt = refreshTokenExpiresAt;
                    _context.RefreshTokens.Update(existingRefreshToken);
                }
                else
                {
                    // Create new refresh token
                    var refreshToken = new RefreshToken
                    {
                        Token = refreshTokenString,
                        UserId = user.Id,
                        ExpiresAt = refreshTokenExpiresAt
                    };
                    _context.RefreshTokens.Add(refreshToken);
                }

                await _context.SaveChangesAsync();

                // Calculate expiration timestamps (Unix time in seconds)
                var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpirationMinutes());
                var accessTokenUnixTime = new DateTimeOffset(accessTokenExpiresAt).ToUnixTimeSeconds();
                var refreshTokenUnixTime = new DateTimeOffset(refreshTokenExpiresAt).ToUnixTimeSeconds();

                var userDto = _mapper.Map<UserDto>(user);
                var response = new LoginResponseDto 
                { 
                    AccessToken = accessToken, 
                    RefreshToken = refreshTokenString, 
                    User = userDto,
                    AccessTokenExpiresIn = accessTokenUnixTime,
                    RefreshTokenExpiresIn = refreshTokenUnixTime
                };
                
                Logger?.LogInformation($"User {user.Email} logged in successfully");
                return Ok(response);
            }, "Login");
        }


        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            return await SafeExecute(async () =>
            {
                var userId = GetUserId();
                await _authService.LogoutAsync(userId);

                Logger?.LogInformation($"User {userId} logged out");
                return Ok(new { message = "Successfully logged out." });
            }, "Logout");
        }

        // Refresh Token Endpoint
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (result == null) return BadRequest(new { message = "Invalid or expired refresh token." });
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            return await SafeExecute(async () =>
            {
                // Additional validation
                if (request == null)
                {
                    Logger?.LogWarning("ForgotPassword: Request is null");
                    return BadRequest(new { message = "Request body is required" });
                }

                var (isValid, err) = ValidateNotEmpty(request.Email, "Email");
                if (!isValid) return err;

                // Validate email format
                if (!request.Email.Contains("@"))
                {
                    return BadRequest(new { message = "Invalid email format" });
                }

                await _authService.ForgotPasswordAsync(request.Email);

                Logger?.LogInformation($"Forgot password request for email: {request.Email}");
                return Ok(new { message = "If an account exists with this email, you will receive password reset instructions." });
            }, "ForgotPassword");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            return await SafeExecute(async () =>
            {
                var (isTokenValid, tokenErr) = ValidateNotEmpty(request.Token, "Reset token");
                if (!isTokenValid) return tokenErr;

                var (isPasswordValid, passwordErr) = ValidateNotEmpty(request.NewPassword, "New password");
                if (!isPasswordValid) return passwordErr;

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest(new { message = "Passwords do not match" });
                }

                if (request.NewPassword.Length < 8)
                {
                    return BadRequest(new { message = "Password must be at least 8 characters long" });
                }

                var result = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

                if (!result)
                {
                    return BadRequest(new { message = "Invalid or expired reset token. Please request a new password reset." });
                }

                Logger?.LogInformation("Password reset successfully");
                return Ok(new { message = "Password has been reset successfully. You can now log in with your new password." });
            }, "ResetPassword");
        }

        [HttpGet("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken(string token)
        {
            return await SafeExecute(async () =>
            {
                var (isValid, err) = ValidateNotEmpty(token, "Reset token");
                if (!isValid) return err;

                var user = await _authService.ValidatePasswordResetTokenAsync(token);

                if (user == null)
                {
                    return BadRequest(new { message = "Invalid or expired reset token" });
                }

                return Ok(new { message = "Token is valid", email = user.Email });
            }, "ValidateResetToken");
        }
    }
}