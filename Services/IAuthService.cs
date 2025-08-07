// Services/IAuthService.cs
using SporBackend.DTOs;

namespace SporBackend.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        string GenerateJwtToken(int userId, string email);
    }
}