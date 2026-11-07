using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.API.DTOs.Announcements;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ILogger<AnnouncementsController> _logger;

        public AnnouncementsController(
            IAnnouncementService announcementService,
            ILogger<AnnouncementsController> logger)
        {
            _announcementService = announcementService;
            _logger = logger;
        }

        // Bütün görünən elanlar (Rola və yazıldığı kurslara uyğun)
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AnnouncementDto>>> GetAll()
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var list = await _announcementService.GetAnnouncementsAsync(currentUserId, currentUserRole);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elanlar siyahısı alınarkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Elanlar alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Tək bir elanın detalları
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AnnouncementDto>> GetById(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var item = await _announcementService.GetByIdAsync(id, currentUserId, currentUserRole);
                if (item == null)
                {
                    return NotFound(new { message = "Elan tapılmadı və ya baxmaq icazəniz yoxdur." });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elan detalları alınarkən xəta baş verdi. Id: {Id}", id);
                return StatusCode(500, new { message = $"Elan alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Elan yaratmaq üçün icazəli kurslar siyahısı (Dropdown üçün)
        [HttpGet("courses")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<ActionResult<IReadOnlyList<AnnouncementCourseOptionDto>>> GetCreatableCourses()
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var courses = await _announcementService.GetCreatableCoursesAsync(currentUserId, currentUserRole);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elan üçün kurslar siyahısı alınarkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Kurslar alına bilmədi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Yeni elan yaratmaq
        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<IActionResult> Create([FromBody] AnnouncementCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var (success, error, id) = await _announcementService.CreateAnnouncementAsync(dto, currentUserId, currentUserRole);
                if (!success)
                {
                    return BadRequest(new { message = error });
                }

                return CreatedAtAction(nameof(GetById), new { id = id!.Value }, new { id = id.Value, message = "Elan uğurla dərc edildi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elan yaradılarkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Elan yaradılarkən verilənlər bazası xətası baş verdi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Elanı redaktə etmək
        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<IActionResult> Update(int id, [FromBody] AnnouncementUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var (success, error) = await _announcementService.UpdateAnnouncementAsync(id, dto, currentUserId, currentUserRole);
                if (!success)
                {
                    return BadRequest(new { message = error });
                }

                return Ok(new { message = "Elan uğurla yeniləndi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elan yenilənərkən xəta baş verdi. Id: {Id}", id);
                return StatusCode(500, new { message = $"Elan yenilənərkən verilənlər bazası xətası baş verdi: {FormatExceptionMessage(ex)}" });
            }
        }

        // Elanı silmək
        [HttpDelete("{id:int}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            try
            {
                var (success, error) = await _announcementService.DeleteAnnouncementAsync(id, currentUserId, currentUserRole);
                if (!success)
                {
                    return BadRequest(new { message = error });
                }

                return Ok(new { message = "Elan uğurla silindi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elan silinərkən xəta baş verdi. Id: {Id}", id);
                return StatusCode(500, new { message = $"Elan silinərkən verilənlər bazası xətası baş verdi: {FormatExceptionMessage(ex)}" });
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
