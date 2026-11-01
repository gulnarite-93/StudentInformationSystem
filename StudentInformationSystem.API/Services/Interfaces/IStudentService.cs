using StudentInformationSystem.API.DTOs.Students;

namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface IStudentService
    {
        Task<IReadOnlyList<StudentDto>> GetAllAsync(string? search = null, int? programmeId = null, int? academicYear = null);
        Task<StudentDto?> GetByIdAsync(int id);
        Task<StudentProfileDto?> GetProfileByUserIdAsync(int userId);
        Task<(bool Success, StudentDto? Student, string? ErrorMessage)> CreateAsync(StudentCreateDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, StudentUpdateDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}
