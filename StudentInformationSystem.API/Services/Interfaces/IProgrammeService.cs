using StudentInformationSystem.API.DTOs.Programmes;

namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface IProgrammeService
    {
        Task<IReadOnlyList<ProgrammeDto>> GetAllAsync();
        Task<ProgrammeDto?> GetByIdAsync(int id);
        Task<ProgrammeDto> CreateAsync(ProgrammeCreateDto dto);
        Task<bool> UpdateAsync(int id, ProgrammeUpdateDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}
