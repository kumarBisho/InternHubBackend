using System;
using System.Threading.Tasks;
using InternMS.Domain.Entities;
using InternMS.Api.DTOs.Authentication;

namespace InternMS.Api.Services.Auth
{
    public interface IAuthService
    {
        Task<User?> ValidateCredentialsAsync(string email, string password);
        Task<User?> ConfirmEmailAsync(string token);
        Task<bool> ApproveUserAsync(Guid userId);
        Task<User?> RegisterUserAsync(string email, string password, string firstName, string lastName, int roleId);
        Task LogoutAsync(string token);
        Task LogoutAsync(Guid userId);
        Task<bool> IsTokenBlacklistedAsync(string token);
        Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<User?> ValidatePasswordResetTokenAsync(string token);

    }
}