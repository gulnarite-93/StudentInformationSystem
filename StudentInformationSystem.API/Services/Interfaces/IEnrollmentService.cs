using StudentInformationSystem.API.DTOs.Enrollments;

namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<(bool Success, EnrollmentDto? Enrollment, string? ErrorMessage)> EnrollAsync(int studentId, int courseId, bool isAdmin = false);
        Task<(bool Success, string? ErrorMessage)> DropAsync(int enrollmentId, int currentUserId, string currentUserRole);
        Task<(bool Success, string? ErrorMessage)> DropByCourseAsync(int studentId, int courseId, int currentUserId, string currentUserRole);
        Task<IReadOnlyList<EnrollmentDto>> GetStudentEnrollmentsAsync(int studentId, int? termId = null, string? status = null);
        Task<IReadOnlyList<EnrolledStudentDto>> GetCourseEnrolledStudentsAsync(int courseId, int currentUserId, string currentUserRole);
        Task<IReadOnlyList<StudentCourseRegistrationDto>> GetAvailableCoursesForStudentAsync(int studentId);
    }
}
