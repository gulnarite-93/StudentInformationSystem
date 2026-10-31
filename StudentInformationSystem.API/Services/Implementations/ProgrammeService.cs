using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.DTOs.Programmes;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Repositories;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Services.Implementations
{
    public class ProgrammeService : IProgrammeService
    {
        private readonly IRepository<Programme> _programmeRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Course> _courseRepo;

        public ProgrammeService(
            IRepository<Programme> programmeRepo,
            IRepository<Department> departmentRepo,
            IRepository<User> userRepo,
            IRepository<Course> courseRepo)
        {
            _programmeRepo = programmeRepo;
            _departmentRepo = departmentRepo;
            _userRepo = userRepo;
            _courseRepo = courseRepo;
        }

        public async Task<IReadOnlyList<ProgrammeDto>> GetAllAsync()
        {
            return await _programmeRepo.Query()
                .Include(p => p.Department)
                .Select(p => new ProgrammeDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department.Name,
                    StudentCount = p.Students.Count,
                    CourseCount = p.Courses.Count
                })
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<ProgrammeDto?> GetByIdAsync(int id)
        {
            return await _programmeRepo.Query()
                .Include(p => p.Department)
                .Where(p => p.Id == id)
                .Select(p => new ProgrammeDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department.Name,
                    StudentCount = p.Students.Count,
                    CourseCount = p.Courses.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProgrammeDto> CreateAsync(ProgrammeCreateDto dto)
        {
            var dept = await _departmentRepo.GetByIdAsync(dto.DepartmentId);
            if (dept == null)
            {
                throw new ArgumentException("Seçilmiş şöbə mövcud deyil.");
            }

            var entity = new Programme
            {
                Name = dto.Name.Trim(),
                Code = dto.Code.Trim().ToUpper(),
                DepartmentId = dto.DepartmentId
            };

            await _programmeRepo.AddAsync(entity);

            return new ProgrammeDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                DepartmentId = entity.DepartmentId,
                DepartmentName = dept.Name,
                StudentCount = 0,
                CourseCount = 0
            };
        }

        public async Task<bool> UpdateAsync(int id, ProgrammeUpdateDto dto)
        {
            var entity = await _programmeRepo.GetByIdAsync(id);
            if (entity == null) return false;

            var dept = await _departmentRepo.GetByIdAsync(dto.DepartmentId);
            if (dept == null)
            {
                throw new ArgumentException("Seçilmiş şöbə mövcud deyil.");
            }

            entity.Name = dto.Name.Trim();
            entity.Code = dto.Code.Trim().ToUpper();
            entity.DepartmentId = dto.DepartmentId;

            await _programmeRepo.UpdateAsync(entity);
            return true;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            var entity = await _programmeRepo.GetByIdAsync(id);
            if (entity == null)
            {
                return (false, "Proqram tapılmadı.");
            }

            var hasStudents = await _userRepo.Query().AnyAsync(u => u.ProgrammeId == id);
            if (hasStudents)
            {
                return (false, "Bu proqramda təhsil alan tələbələr olduğu üçün silinə bilməz.");
            }

            var hasCourses = await _courseRepo.Query().AnyAsync(c => c.ProgrammeId == id);
            if (hasCourses)
            {
                return (false, "Bu proqrama bağlı kurslar olduğu üçün silinə bilməz.");
            }

            await _programmeRepo.DeleteAsync(entity);
            return (true, null);
        }
    }
}
