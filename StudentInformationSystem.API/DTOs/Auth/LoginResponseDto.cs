namespace StudentInformationSystem.API.DTOs.Auth
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        public CurrentUserDto? User { get; set; }
    }
}
