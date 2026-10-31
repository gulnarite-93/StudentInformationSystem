using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.API.DTOs.Programmes;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    public class ProgrammesController : ControllerBase
    {
        private readonly IProgrammeService _programmeService;

        public ProgrammesController(IProgrammeService programmeService)
        {
            _programmeService = programmeService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProgrammeDto>>> GetAll()
        {
            var programmes = await _programmeService.GetAllAsync();
            return Ok(programmes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProgrammeDto>> GetById(int id)
        {
            var programme = await _programmeService.GetByIdAsync(id);
            if (programme == null)
            {
                return NotFound(new { message = "Proqram tapılmadı." });
            }

            return Ok(programme);
        }

        [HttpPost]
        public async Task<ActionResult<ProgrammeDto>> Create([FromBody] ProgrammeCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _programmeService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProgrammeUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _programmeService.UpdateAsync(id, dto);
                if (!updated)
                {
                    return NotFound(new { message = "Proqram tapılmadı." });
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _programmeService.DeleteAsync(id);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }
    }
}
