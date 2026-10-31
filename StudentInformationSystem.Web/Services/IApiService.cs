using StudentInformationSystem.Web.Models;

namespace StudentInformationSystem.Web.Services
{
    public interface IApiService
    {
        Task<ApiLoginResponseDto> LoginAsync(string usernameOrEmail, string password);
        Task<T?> GetAsync<T>(string endpoint);
        Task<TResult?> PostAsync<TRequest, TResult>(string endpoint, TRequest data);
        Task<(bool Success, TResult? Data, string? ErrorMessage)> PostWithResultAsync<TRequest, TResult>(string endpoint, TRequest data);
        Task<bool> PutAsync<TRequest>(string endpoint, TRequest data);
        Task<(bool Success, string? ErrorMessage)> PutWithResultAsync<TRequest>(string endpoint, TRequest data);
        Task<bool> DeleteAsync(string endpoint);
        Task<(bool Success, string? ErrorMessage)> DeleteWithResultAsync(string endpoint);
    }
}
