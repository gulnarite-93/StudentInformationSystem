using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.API.DTOs.Grades;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;
        private readonly ILogger<GradesController> _logger;

        public GradesController(IGradeService gradeService, ILogger<GradesController> logger)
        {
            _gradeService = gradeService;
            _logger = logger;
        }

        // Müəllimin və ya adminin fənlər üzrə qiymətləndirmə xülasəsi
        [HttpGet("teacher-courses")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<ActionResult<IReadOnlyList<TeacherGradeCourseSummaryDto>>> GetTeacherCourses()
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var list = await _gradeService.GetTeacherCoursesSummaryAsync(currentUserId, currentUserRole);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müəllim kursları alınarkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Fənlər siyahısı alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Fənn üzrə bütün tələbələrin qiymət cədvəli
        [HttpGet("course/{courseId:int}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<ActionResult<CourseGradesDto>> GetCourseGrades(int courseId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var result = await _gradeService.GetCourseGradesAsync(courseId, currentUserId, currentUserRole);
                if (result == null)
                {
                    return NotFound(new { message = "Fənn tapılmadı və ya bu fənnə baxmaq hüququnuz yoxdur." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kurs qiymətləri alınarkən xəta baş verdi. CourseId: {CourseId}", courseId);
                return StatusCode(500, new { message = $"Qiymətlər alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Qiymətin saxlanılması və ya yenilənməsi
        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<IActionResult> SaveGrade([FromBody] GradeSaveDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var (success, error) = await _gradeService.SaveGradeAsync(dto, currentUserId, currentUserRole);
                if (!success)
                {
                    return BadRequest(new { message = error });
                }

                return Ok(new { message = "Qiymət uğurla yadda saxlanıldı." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Qiymət saxlanılarkən xəta baş verdi. EnrollmentId: {EnrollmentId}", dto.EnrollmentId);
                return StatusCode(500, new { message = $"Qiymət saxlanılarkən verilənlər bazası xətası baş verdi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Tələbə üçün: Öz qiymətlər siyahısı
        [HttpGet("my-grades")]
        [Authorize(Roles = UserRoles.Student)]
        public async Task<ActionResult<IReadOnlyList<GradeDto>>> GetMyGrades()
        {
            var currentUserId = GetCurrentUserId();

            try
            {
                var grades = await _gradeService.GetStudentGradesAsync(currentUserId, currentUserId, UserRoles.Student);
                return Ok(grades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tələbə qiymətləri alınarkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Qiymətlər alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Admin və ya Müəllim üçün: Tələbənin qiymətləri
        [HttpGet("student/{studentId:int}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<ActionResult<IReadOnlyList<GradeDto>>> GetStudentGrades(int studentId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var grades = await _gradeService.GetStudentGradesAsync(studentId, currentUserId, currentUserRole);
                return Ok(grades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tələbə qiymətləri alınarkən xəta baş verdi. StudentId: {StudentId}", studentId);
                return StatusCode(500, new { message = $"Tələbə qiymətləri alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Tələbə üçün: Öz rəsmi transkripti
        [HttpGet("transcript")]
        [Authorize(Roles = UserRoles.Student)]
        public async Task<ActionResult<StudentTranscriptDto>> GetMyTranscript()
        {
            var currentUserId = GetCurrentUserId();

            try
            {
                var transcript = await _gradeService.GetStudentTranscriptAsync(currentUserId, currentUserId, UserRoles.Student);
                if (transcript == null)
                {
                    return NotFound(new { message = "Transkript tapılmadı." });
                }

                return Ok(transcript);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transkript alınarkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Transkript alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Admin və ya Müəllim üçün: Tələbənin rəsmi transkripti
        [HttpGet("transcript/student/{studentId:int}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<ActionResult<StudentTranscriptDto>> GetStudentTranscript(int studentId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var transcript = await _gradeService.GetStudentTranscriptAsync(studentId, currentUserId, currentUserRole);
                if (transcript == null)
                {
                    return NotFound(new { message = "Transkript tapılmadı." });
                }

                return Ok(transcript);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tələbə transkripti alınarkən xəta baş verdi. StudentId: {StudentId}", studentId);
                return StatusCode(500, new { message = $"Transkript alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Tələbə üçün: Öz akademik paneli (Dashboard)
        [HttpGet("dashboard")]
        [Authorize(Roles = UserRoles.Student)]
        public async Task<ActionResult<StudentDashboardDto>> GetMyDashboard()
        {
            var currentUserId = GetCurrentUserId();

            try
            {
                var dashboard = await _gradeService.GetStudentDashboardAsync(currentUserId, currentUserId, UserRoles.Student);
                if (dashboard == null)
                {
                    return NotFound(new { message = "Akademik panel məlumatları tapılmadı." });
                }

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Akademik panel məlumatları alınarkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Akademik panel alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Admin və ya Müəllim üçün: Tələbənin akademik paneli (Dashboard)
        [HttpGet("dashboard/student/{studentId:int}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<ActionResult<StudentDashboardDto>> GetStudentDashboard(int studentId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var dashboard = await _gradeService.GetStudentDashboardAsync(studentId, currentUserId, currentUserRole);
                if (dashboard == null)
                {
                    return NotFound(new { message = "Tələbənin akademik paneli tapılmadı." });
                }

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tələbə paneli alınarkən xəta baş verdi. StudentId: {StudentId}", studentId);
                return StatusCode(500, new { message = $"Panel məlumatları alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value
                ?? string.Empty;
        }

        private static string FormatExceptionMessage(Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException != null)
            {
                msg += $" (Daxili səbəb: {ex.InnerException.Message})";
            }
            return msg;
        }
    }
}
