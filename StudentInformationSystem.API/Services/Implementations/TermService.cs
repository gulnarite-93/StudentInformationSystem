using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.DTOs.Terms;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Repositories;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Services.Implementations
{
    public class TermService : ITermService
    {
        private readonly IRepository<Term> _termRepo;
        private readonly IRepository<Course> _courseRepo;

        public TermService(
            IRepository<Term> termRepo,
            IRepository<Course> courseRepo)
        {
            _termRepo = termRepo;
            _courseRepo = courseRepo;
        }

        public async Task<IReadOnlyList<TermDto>> GetAllAsync()
        {
            return await _termRepo.Query()
                .Select(t => new TermDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    IsActive = t.IsActive,
                    CourseCount = t.Courses.Count
                })
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
        }

        public async Task<TermDto?> GetByIdAsync(int id)
        {
            return await _termRepo.Query()
                .Where(t => t.Id == id)
                .Select(t => new TermDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    IsActive = t.IsActive,
                    CourseCount = t.Courses.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TermDto> CreateAsync(TermCreateDto dto)
        {
            var entity = new Term
            {
                Name = dto.Name.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive
            };

            await _termRepo.AddAsync(entity);

            return new TermDto
            {
                Id = entity.Id,
                Name = entity.Name,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                IsActive = entity.IsActive,
                CourseCount = 0
            };
        }

        public async Task<bool> UpdateAsync(int id, TermUpdateDto dto)
        {
            var entity = await _termRepo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Name = dto.Name.Trim();
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.IsActive = dto.IsActive;

            await _termRepo.UpdateAsync(entity);
            return true;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            var entity = await _termRepo.GetByIdAsync(id);
            if (entity == null)
            {
                return (false, "Akademik dövr tapılmadı.");
            }

            var hasCourses = await _courseRepo.Query().AnyAsync(c => c.TermId == id);
            if (hasCourses)
            {
                return (false, "Bu akademik dövrə aid kurslar olduğu üçün silinə bilməz.");
            }

            await _termRepo.DeleteAsync(entity);
            return (true, null);
        }
    }
}
