using StudentInformationSystem.API.DTOs.Departments;

namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IReadOnlyList<DepartmentDto>> GetAllAsync();
        Task<DepartmentDto?> GetByIdAsync(int id);
        Task<DepartmentDto> CreateAsync(DepartmentCreateDto dto);
        Task<bool> UpdateAsync(int id, DepartmentUpdateDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}
