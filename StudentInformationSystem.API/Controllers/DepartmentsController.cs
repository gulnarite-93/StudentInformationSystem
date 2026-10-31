using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.API.DTOs.Departments;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetById(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound(new { message = "Şöbə tapılmadı." });
            }

            return Ok(department);
        }

        [HttpPost]
        public async Task<ActionResult<DepartmentDto>> Create([FromBody] DepartmentCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _departmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _departmentService.UpdateAsync(id, dto);
            if (!updated)
            {
                return NotFound(new { message = "Şöbə tapılmadı." });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _departmentService.DeleteAsync(id);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }
    }
}
