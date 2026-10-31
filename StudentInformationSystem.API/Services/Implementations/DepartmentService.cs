using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.DTOs.Departments;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Repositories;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Programme> _programmeRepo;
        private readonly IRepository<User> _userRepo;

        public DepartmentService(
            IRepository<Department> departmentRepo,
            IRepository<Programme> programmeRepo,
            IRepository<User> userRepo)
        {
            _departmentRepo = departmentRepo;
            _programmeRepo = programmeRepo;
            _userRepo = userRepo;
        }

        public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync()
        {
            return await _departmentRepo.Query()
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Code = d.Code,
                    Description = d.Description,
                    ProgrammeCount = d.Programmes.Count,
                    TeacherCount = d.Teachers.Count
                })
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            var dept = await _departmentRepo.Query()
                .Where(d => d.Id == id)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Code = d.Code,
                    Description = d.Description,
                    ProgrammeCount = d.Programmes.Count,
                    TeacherCount = d.Teachers.Count
                })
                .FirstOrDefaultAsync();

            return dept;
        }

        public async Task<DepartmentDto> CreateAsync(DepartmentCreateDto dto)
        {
            var entity = new Department
            {
                Name = dto.Name.Trim(),
                Code = dto.Code.Trim().ToUpper(),
                Description = dto.Description?.Trim()
            };

            await _departmentRepo.AddAsync(entity);

            return new DepartmentDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                Description = entity.Description,
                ProgrammeCount = 0,
                TeacherCount = 0
            };
        }

        public async Task<bool> UpdateAsync(int id, DepartmentUpdateDto dto)
        {
            var entity = await _departmentRepo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Name = dto.Name.Trim();
            entity.Code = dto.Code.Trim().ToUpper();
            entity.Description = dto.Description?.Trim();

            await _departmentRepo.UpdateAsync(entity);
            return true;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            var entity = await _departmentRepo.GetByIdAsync(id);
            if (entity == null)
            {
                return (false, "Şöbə tapılmadı.");
            }

            var hasProgrammes = await _programmeRepo.Query().AnyAsync(p => p.DepartmentId == id);
            if (hasProgrammes)
            {
                return (false, "Bu şöbəyə bağlı təhsil proqramları olduğu üçün silinə bilməz. Əvvəlcə həmin proqramları silin və ya başqa şöbəyə keçirin.");
            }

            var hasTeachers = await _userRepo.Query().AnyAsync(u => u.DepartmentId == id);
            if (hasTeachers)
            {
                return (false, "Bu şöbəyə bağlı müəllimlər olduğu üçün silinə bilməz.");
            }

            await _departmentRepo.DeleteAsync(entity);
            return (true, null);
        }
    }
}
