using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.API.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "İstifadəçi adı və ya e-poçt mütləqdir.")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə mütləqdir.")]
        public string Password { get; set; } = string.Empty;
    }
}
