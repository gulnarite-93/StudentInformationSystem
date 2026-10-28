using StudentInformationSystem.API.DTOs.Auth;

namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<CurrentUserDto?> GetCurrentUserAsync(int userId);
    }
}
