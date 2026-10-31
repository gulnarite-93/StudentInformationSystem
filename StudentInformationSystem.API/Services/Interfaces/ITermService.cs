using StudentInformationSystem.API.DTOs.Terms;

namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface ITermService
    {
        Task<IReadOnlyList<TermDto>> GetAllAsync();
        Task<TermDto?> GetByIdAsync(int id);
        Task<TermDto> CreateAsync(TermCreateDto dto);
        Task<bool> UpdateAsync(int id, TermUpdateDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}
