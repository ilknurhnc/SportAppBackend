// Services/AuthService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SporBackend.Data;
using SporBackend.DTOs;
using SporBackend.Models;

namespace SporBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly SporContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(SporContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Geçersiz email veya şifre"
                    };
                }

                var token = GenerateJwtToken(user.Id, user.Email);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Giriş başarılı",
                    User = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email
                    },
                    Token = token
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Giriş sırasında hata oluştu"
                };
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Email kontrolü
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Bu email adresi zaten kullanılıyor"
                    };
                }

                var user = new User
                {
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    PasswordHash = HashPassword(registerDto.Password),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Kayıt başarılı"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Kayıt sırasında hata oluştu"
                };
            }
        }

        public string GenerateJwtToken(int userId, string email)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("userId", userId.ToString()),
                    new Claim(ClaimTypes.Email, email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            var hash = pbkdf2.GetBytes(32);

            var hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            var salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            var hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return false;
            }

            return true;
        }
    }
}