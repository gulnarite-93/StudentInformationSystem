using StudentInformationSystem.API.Entities;

namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
