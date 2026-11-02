using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.API.DTOs.Attendances;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger)
        {
            _attendanceService = attendanceService;
            _logger = logger;
        }

        // Tələbənin bütün kurslar üzrə davamiyyət xülasəsi və tarixçəsi
        [HttpGet("my")]
        [Authorize(Roles = UserRoles.Student)]
        public async Task<ActionResult<IReadOnlyList<StudentCourseAttendanceSummaryDto>>> GetMyAttendance()
        {
            var studentId = GetCurrentUserId();
            if (studentId == 0) return Unauthorized();

            try
            {
                var result = await _attendanceService.GetStudentAttendanceAsync(studentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tələbə davamiyyəti alınarkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Davamiyyət məlumatları alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Kurs üzrə keçirilmiş bütün dərs sessiyalarının xülasəsi
        [HttpGet("course/{courseId:int}/sessions")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<ActionResult<IReadOnlyList<LessonSessionSummaryDto>>> GetCourseSessions(int courseId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var sessions = await _attendanceService.GetCourseSessionsAsync(courseId, currentUserId, currentUserRole);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kurs sessiyaları alınarkən xəta baş verdi. CourseId: {CourseId}", courseId);
                return StatusCode(500, new { message = $"Sessiyalar siyahısı alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Yeni dərs sessiyası yaratmaq üçün kursa yazılmış tələbələrin şablon siyahısı
        [HttpGet("course/{courseId:int}/template")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<ActionResult<LessonSessionDetailsDto>> GetSessionTemplate(int courseId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var template = await _attendanceService.GetNewSessionTemplateAsync(courseId, currentUserId, currentUserRole);
                return Ok(template);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dərs şablonu alınarkən xəta baş verdi. CourseId: {CourseId}", courseId);
                return StatusCode(500, new { message = $"Dərs şablonu alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Müəyyən tarixdəki dərs sessiyasının təfərrüatları (redaktə üçün)
        [HttpGet("course/{courseId:int}/session")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<ActionResult<LessonSessionDetailsDto>> GetSessionDetails(int courseId, [FromQuery] DateTime date)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var session = await _attendanceService.GetSessionDetailsAsync(courseId, date, currentUserId, currentUserRole);
                if (session == null)
                {
                    return NotFound(new { message = "Göstərilən tarixdə dərs sessiyası tapılmadı." });
                }

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sessiya detalları alınarkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Sessiya detalları alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Yeni dərs sessiyasının və davamiyyət qeydlərinin saxlanılması
        [HttpPost("session")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<IActionResult> CreateSession([FromBody] CreateLessonSessionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var (success, errorMessage) = await _attendanceService.CreateSessionAsync(dto, currentUserId, currentUserRole);
                if (!success)
                {
                    return BadRequest(new { message = errorMessage });
                }

                return Ok(new { message = "Dərs sessiyası və davamiyyət qeydləri uğurla yadda saxlanıldı." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dərs sessiyası saxlanılarkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Davamiyyət saxlanılarkən verilənlər bazası xətası baş verdi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Əvvəlki dərs sessiyasının davamiyyətinin redaktəsi
        [HttpPut("session")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<IActionResult> UpdateSession([FromBody] UpdateLessonSessionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var (success, errorMessage) = await _attendanceService.UpdateSessionAsync(dto, currentUserId, currentUserRole);
                if (!success)
                {
                    return BadRequest(new { message = errorMessage });
                }

                return Ok(new { message = "Dərs sessiyasının davamiyyəti uğurla yeniləndi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dərs sessiyası yenilənərkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Davamiyyət yenilənərkən verilənlər bazası xətası baş verdi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Sessiyanın silinməsi
        [HttpDelete("course/{courseId:int}/session")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<IActionResult> DeleteSession(int courseId, [FromQuery] DateTime date)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var (success, errorMessage) = await _attendanceService.DeleteSessionAsync(courseId, date, currentUserId, currentUserRole);
                if (!success)
                {
                    return BadRequest(new { message = errorMessage });
                }

                return Ok(new { message = "Dərs sessiyası silindi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dərs sessiyası silinərkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Sessiya silinərkən xəta baş verdi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Admin üçün: İstənilən tələbənin davamiyyəti
        [HttpGet("student/{studentId:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<IReadOnlyList<StudentCourseAttendanceSummaryDto>>> GetStudentAttendance(int studentId)
        {
            try
            {
                var result = await _attendanceService.GetStudentAttendanceAsync(studentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tələbə davamiyyəti alınarkən xəta baş verdi. StudentId: {StudentId}", studentId);
                return StatusCode(500, new { message = $"Tələbə davamiyyəti alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
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
    }
}
