using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "İstifadəçi adı və ya e-poçt mütləqdir.")]
        [Display(Name = "İstifadəçi adı və ya E-poçt")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə mütləqdir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Məni xatırla")]
        public bool RememberMe { get; set; }
    }
}
