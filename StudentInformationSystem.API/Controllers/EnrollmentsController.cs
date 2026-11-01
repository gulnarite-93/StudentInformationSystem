using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.API.DTOs.Enrollments;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        // Tələbənin cari aktiv və ya keçmiş qeydiyyatları
        [HttpGet("my")]
        [Authorize(Roles = UserRoles.Student)]
        public async Task<ActionResult<IReadOnlyList<EnrollmentDto>>> GetMyEnrollments(
            [FromQuery] int? termId = null,
            [FromQuery] string? status = null)
        {
            var studentId = GetCurrentUserId();
            if (studentId == 0) return Unauthorized();

            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(studentId, termId, status);
            return Ok(enrollments);
        }

        // Tələbə üçün aktiv semestrdə seçilə biləcək fənlər (artıq yazılıb-yazılmadığı statusu ilə)
        [HttpGet("available")]
        [Authorize(Roles = UserRoles.Student)]
        public async Task<ActionResult<IReadOnlyList<StudentCourseRegistrationDto>>> GetAvailableCourses()
        {
            var studentId = GetCurrentUserId();
            if (studentId == 0) return Unauthorized();

            var courses = await _enrollmentService.GetAvailableCoursesForStudentAsync(studentId);
            return Ok(courses);
        }

        // Kursa yazılma (Tələbə özü və ya Admin istənilən tələbə üçün)
        [HttpPost("enroll")]
        [Authorize]
        public async Task<ActionResult<EnrollmentDto>> Enroll([FromBody] EnrollRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            int targetStudentId;
            bool isAdmin = currentUserRole == UserRoles.Admin;

            if (isAdmin)
            {
                if (!dto.StudentId.HasValue || dto.StudentId.Value <= 0)
                {
                    return BadRequest(new { message = "Admin tərəfindən qeydiyyat üçün tələbə seçilməlidir (StudentId tələb olunur)." });
                }
                targetStudentId = dto.StudentId.Value;
            }
            else if (currentUserRole == UserRoles.Student)
            {
                // Qayda: Tələbə yalnız öz adından yaza bilər
                targetStudentId = currentUserId;
            }
            else
            {
                return Forbid();
            }

            var (success, enrollment, errorMessage) = await _enrollmentService.EnrollAsync(targetStudentId, dto.CourseId, isAdmin);
            if (!success || enrollment == null)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(enrollment);
        }

        // Kursdan çıxma (Drop) - enrollmentId vasitəsilə
        [HttpPost("{id:int}/drop")]
        [Authorize]
        public async Task<IActionResult> Drop(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var (success, errorMessage) = await _enrollmentService.DropAsync(id, currentUserId, currentUserRole);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "Kursa qeydiyyat uğurla ləğv edildi." });
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var (success, errorMessage) = await _enrollmentService.DropAsync(id, currentUserId, currentUserRole);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }

        // Kursdan çıxma (Drop) - courseId vasitəsilə
        [HttpPost("drop-course")]
        [Authorize]
        public async Task<IActionResult> DropCourse([FromBody] DropRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            int targetStudentId;
            if (currentUserRole == UserRoles.Admin)
            {
                if (!dto.StudentId.HasValue || dto.StudentId.Value <= 0)
                {
                    return BadRequest(new { message = "Tələbə ID-si qeyd edilməlidir." });
                }
                targetStudentId = dto.StudentId.Value;
            }
            else if (currentUserRole == UserRoles.Student)
            {
                targetStudentId = currentUserId;
            }
            else
            {
                return Forbid();
            }

            var (success, errorMessage) = await _enrollmentService.DropByCourseAsync(targetStudentId, dto.CourseId, currentUserId, currentUserRole);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "Kursa qeydiyyat uğurla ləğv edildi." });
        }

        // Admin üçün: İstənilən tələbənin qeydiyyatları
        [HttpGet("student/{studentId:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<IReadOnlyList<EnrollmentDto>>> GetStudentEnrollments(
            int studentId,
            [FromQuery] int? termId = null,
            [FromQuery] string? status = null)
        {
            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(studentId, termId, status);
            return Ok(enrollments);
        }

        // Müəllim və Admin üçün: Kursa yazılmış tələbələrin siyahısı
        [HttpGet("course/{courseId:int}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<ActionResult<IReadOnlyList<EnrolledStudentDto>>> GetCourseEnrolledStudents(int courseId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var students = await _enrollmentService.GetCourseEnrolledStudentsAsync(courseId, currentUserId, currentUserRole);
            return Ok(students);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
