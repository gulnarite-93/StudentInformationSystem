using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.DTOs.Auth;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Repositories;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(
            IRepository<User> userRepository,
            IPasswordHasher _passwordHasher,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            this._passwordHasher = _passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.Query()
                .Include(u => u.Programme)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

            if (user == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "İstifadəçi adı və ya şifrə yanlışdır."
                };
            }

            var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "İstifadəçi adı və ya şifrə yanlışdır."
                };
            }

            var token = _tokenService.GenerateToken(user);

            var userDto = new CurrentUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                StudentNumber = user.StudentNumber,
                AcademicYear = user.AcademicYear,
                ProgrammeId = user.ProgrammeId,
                ProgrammeName = user.Programme?.Name,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name,
                Title = user.Title
            };

            return new LoginResponseDto
            {
                Success = true,
                Token = token,
                Message = "Uğurla daxil oldunuz.",
                User = userDto
            };
        }

        public async Task<CurrentUserDto?> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepository.Query()
                .Include(u => u.Programme)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new CurrentUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                StudentNumber = user.StudentNumber,
                AcademicYear = user.AcademicYear,
                ProgrammeId = user.ProgrammeId,
                ProgrammeName = user.Programme?.Name,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name,
                Title = user.Title
            };
        }
    }
}
