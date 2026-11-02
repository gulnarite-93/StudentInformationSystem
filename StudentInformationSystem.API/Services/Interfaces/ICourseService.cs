using StudentInformationSystem.API.DTOs.Courses;

namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface ICourseService
    {
        Task<IReadOnlyList<CourseDto>> GetAllAsync(string? search = null, int? termId = null, int? programmeId = null, int? teacherId = null);
        Task<CourseDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<TeacherSimpleDto>> GetTeachersListAsync();
        Task<(bool Success, CourseDto? Course, string? ErrorMessage)> CreateAsync(CourseCreateDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, CourseUpdateDto dto);
        Task<(bool Success, string? ErrorMessage)> AssignTeacherAsync(int courseId, int? teacherId);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}
