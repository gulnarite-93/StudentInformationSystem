using System.Net.Http.Headers;
using System.Text.Json;
using StudentInformationSystem.Web.Models;

namespace StudentInformationSystem.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7109";
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            _httpClient.BaseAddress = new Uri(baseUrl);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private void AttachJwtToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("JwtToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<ApiLoginResponseDto> LoginAsync(string usernameOrEmail, string password)
        {
            try
            {
                var payload = new { UsernameOrEmail = usernameOrEmail, Password = password };
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", payload);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiLoginResponseDto>(content, _jsonOptions);

                return result ?? new ApiLoginResponseDto
                {
                    Success = false,
                    Message = "API-dən cavab alına bilmədi."
                };
            }
            catch (Exception ex)
            {
                return new ApiLoginResponseDto
                {
                    Success = false,
                    Message = $"API ilə əlaqə qurarkən xəta baş verdi: {ex.Message}"
                };
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AttachJwtToken();
            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        public async Task<TResult?> PostAsync<TRequest, TResult>(string endpoint, TRequest data)
        {
            AttachJwtToken();
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResult>(content, _jsonOptions);
        }

        public async Task<(bool Success, TResult? Data, string? ErrorMessage)> PostWithResultAsync<TRequest, TResult>(string endpoint, TRequest data)
        {
            AttachJwtToken();
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<TResult>(content, _jsonOptions);
                return (true, result, null);
            }

            var error = ExtractErrorMessage(content, response, "Əlavə edilərkən xəta baş verdi.");
            return (false, default, error);
        }

        public async Task<bool> PutAsync<TRequest>(string endpoint, TRequest data)
        {
            AttachJwtToken();
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string? ErrorMessage)> PutWithResultAsync<TRequest>(string endpoint, TRequest data)
        {
            AttachJwtToken();
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var content = await response.Content.ReadAsStringAsync();
            var error = ExtractErrorMessage(content, response, "Yeniləmə zamanı xəta baş verdi.");
            return (false, error);
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            AttachJwtToken();
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteWithResultAsync(string endpoint)
        {
            AttachJwtToken();
            var response = await _httpClient.DeleteAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var content = await response.Content.ReadAsStringAsync();
            var error = ExtractErrorMessage(content, response, "Silinmə zamanı xəta baş verdi.");
            return (false, error);
        }

        private static string ExtractErrorMessage(string content, HttpResponseMessage response, string fallback)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return $"{fallback} (HTTP {(int)response.StatusCode})";
            }

            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.TryGetProperty("message", out var msg) && !string.IsNullOrWhiteSpace(msg.GetString()))
                {
                    return msg.GetString()!;
                }

                if (root.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString()))
                {
                    return detail.GetString()!;
                }

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var errorList = new List<string>();
                    foreach (var prop in errors.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                var text = item.GetString();
                                if (!string.IsNullOrEmpty(text)) errorList.Add(text);
                            }
                        }
                        else
                        {
                            var text = prop.Value.GetString();
                            if (!string.IsNullOrEmpty(text)) errorList.Add(text);
                        }
                    }

                    if (errorList.Count > 0)
                    {
                        return string.Join("; ", errorList);
                    }
                }

                if (root.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString()))
                {
                    return title.GetString()!;
                }
            }
            catch
            {
            }

            if (content.Length < 300 && !content.TrimStart().StartsWith("<"))
            {
                return content.Trim();
            }

            return $"{fallback} (HTTP {(int)response.StatusCode})";
        }
    }
}
