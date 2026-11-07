using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.API.DTOs.Courses;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int? termId = null,
            [FromQuery] int? programmeId = null,
            [FromQuery] int? teacherId = null)
        {
            var courses = await _courseService.GetAllAsync(search, termId, programmeId, teacherId);
            return Ok(courses);
        }

        [HttpGet("my-courses")]
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetMyCourses()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var teacherId))
            {
                return Unauthorized();
            }

            var courses = await _courseService.GetAllAsync(teacherId: teacherId);
            return Ok(courses);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<CourseDto>> GetById(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound(new { message = "Kurs tapılmadı." });
            }

            return Ok(course);
        }

        [HttpGet("teachers")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<IReadOnlyList<TeacherSimpleDto>>> GetTeachers()
        {
            var teachers = await _courseService.GetTeachersListAsync();
            return Ok(teachers);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<CourseDto>> Create([FromBody] CourseCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, course, errorMessage) = await _courseService.CreateAsync(dto);
            if (!success || course == null)
            {
                return BadRequest(new { message = errorMessage });
            }

            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, errorMessage) = await _courseService.UpdateAsync(id, dto);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }

        [HttpPut("{id:int}/assign-teacher")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> AssignTeacher(int id, [FromBody] AssignTeacherDto dto)
        {
            var (success, errorMessage) = await _courseService.AssignTeacherAsync(id, dto.TeacherId);
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
            var (success, errorMessage) = await _courseService.DeleteAsync(id);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }
    }
}
