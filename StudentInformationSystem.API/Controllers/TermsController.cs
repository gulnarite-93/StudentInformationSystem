using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.API.DTOs.Terms;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    public class TermsController : ControllerBase
    {
        private readonly ITermService _termService;

        public TermsController(ITermService termService)
        {
            _termService = termService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<TermDto>>> GetAll()
        {
            var terms = await _termService.GetAllAsync();
            return Ok(terms);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TermDto>> GetById(int id)
        {
            var term = await _termService.GetByIdAsync(id);
            if (term == null)
            {
                return NotFound(new { message = "Akademik dövr tapılmadı." });
            }

            return Ok(term);
        }

        [HttpPost]
        public async Task<ActionResult<TermDto>> Create([FromBody] TermCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _termService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TermUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _termService.UpdateAsync(id, dto);
            if (!updated)
            {
                return NotFound(new { message = "Akademik dövr tapılmadı." });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _termService.DeleteAsync(id);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }
    }
}
