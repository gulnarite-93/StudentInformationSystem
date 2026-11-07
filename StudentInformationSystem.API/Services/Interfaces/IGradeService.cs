using StudentInformationSystem.API.DTOs.Grades;

namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface IGradeService
    {
        /// <summary>
        /// Müəllimin tədris etdiyi fənlər və hər fənn üzrə qiymətləndirmə xülasəsi
        /// </summary>
        Task<IReadOnlyList<TeacherGradeCourseSummaryDto>> GetTeacherCoursesSummaryAsync(int teacherId, string role);

        /// <summary>
        /// Seçilmiş fənn üzrə bütün tələbələrin qiymət cədvəli
        /// </summary>
        Task<CourseGradesDto?> GetCourseGradesAsync(int courseId, int currentUserId, string currentUserRole);

        /// <summary>
        /// Tələbənin fənn üzrə qiymətini yadda saxlamaq və ya redaktə etmək
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> SaveGradeAsync(GradeSaveDto dto, int currentUserId, string currentUserRole);

        /// <summary>
        /// Tələbənin bütün fənlər üzrə qiymətlər siyahısı ("Qiymətlərim" səhifəsi)
        /// </summary>
        Task<IReadOnlyList<GradeDto>> GetStudentGradesAsync(int studentId, int currentUserId, string currentUserRole);

        /// <summary>
        /// Tələbənin rəsmi transkript məlumatları və GPA göstəricisi
        /// </summary>
        Task<StudentTranscriptDto?> GetStudentTranscriptAsync(int studentId, int currentUserId, string currentUserRole);

        /// <summary>
        /// Tələbənin akademik paneli üçün GPA, kreditlər, davamiyyət %, elanlar və cari kurslar
        /// </summary>
        Task<StudentDashboardDto?> GetStudentDashboardAsync(int studentId, int currentUserId, string currentUserRole);
    }
}
