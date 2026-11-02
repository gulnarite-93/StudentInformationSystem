using StudentInformationSystem.API.DTOs.Attendances;

namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<IReadOnlyList<LessonSessionSummaryDto>> GetCourseSessionsAsync(int courseId, int currentUserId, string currentUserRole);
        Task<LessonSessionDetailsDto?> GetSessionDetailsAsync(int courseId, DateTime lessonDate, int currentUserId, string currentUserRole);
        Task<LessonSessionDetailsDto> GetNewSessionTemplateAsync(int courseId, int currentUserId, string currentUserRole);
        Task<(bool Success, string? ErrorMessage)> CreateSessionAsync(CreateLessonSessionDto dto, int currentUserId, string currentUserRole);
        Task<(bool Success, string? ErrorMessage)> UpdateSessionAsync(UpdateLessonSessionDto dto, int currentUserId, string currentUserRole);
        Task<(bool Success, string? ErrorMessage)> DeleteSessionAsync(int courseId, DateTime lessonDate, int currentUserId, string currentUserRole);
        Task<IReadOnlyList<StudentCourseAttendanceSummaryDto>> GetStudentAttendanceAsync(int studentId);
    }
}
