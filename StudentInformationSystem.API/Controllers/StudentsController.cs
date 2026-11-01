using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.API.DTOs.Students;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int? programmeId = null,
            [FromQuery] int? academicYear = null)
        {
            var students = await _studentService.GetAllAsync(search, programmeId, academicYear);
            return Ok(students);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<StudentDto>> GetById(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound(new { message = "Tələbə tapılmadı." });
            }

            return Ok(student);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<StudentProfileDto>> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var profile = await _studentService.GetProfileByUserIdAsync(userId);
            if (profile == null)
            {
                return NotFound(new { message = "Profil məlumatı tapılmadı." });
            }

            return Ok(profile);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<StudentDto>> Create([FromBody] StudentCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, student, errorMessage) = await _studentService.CreateAsync(dto);
            if (!success || student == null)
            {
                return BadRequest(new { message = errorMessage });
            }

            return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, errorMessage) = await _studentService.UpdateAsync(id, dto);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _studentService.DeleteAsync(id);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }
    }
}
