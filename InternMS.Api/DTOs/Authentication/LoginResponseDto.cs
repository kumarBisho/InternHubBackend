using InternMS.Api.DTOs.Users;

namespace InternMS.Api.DTOs.Authentication
{
    public class LoginResponseDto
    {
      public string AccessToken { get; set; } = string.Empty;
      public string RefreshToken { get; set; } = string.Empty;
      public UserDto User { get; set; } = default!;
      /// <summary>Access token expiration time in Unix timestamp (seconds)</summary>
      public long AccessTokenExpiresIn { get; set; }
      /// <summary>Refresh token expiration time in Unix timestamp (seconds)</summary>
      public long RefreshTokenExpiresIn { get; set; }
    }
}