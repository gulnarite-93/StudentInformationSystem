using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.API.DTOs.Students
{
    public class StudentCreateDto
    {
        [Required(ErrorMessage = "İstifadəçi adı mütləqdir.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "İstifadəçi adı 3-50 simvol aralığında olmalıdır.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-poçt ünvanı mütləqdir.")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt formatı daxil edin.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə mütləqdir.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifrə ən azı 6 simvol olmalıdır.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad mütləqdir.")]
        [StringLength(50, ErrorMessage = "Ad 50 simvoldan çox ola bilməz.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad mütləqdir.")]
        [StringLength(50, ErrorMessage = "Soyad 50 simvoldan çox ola bilməz.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tələbə nömrəsi mütləqdir.")]
        [StringLength(30, ErrorMessage = "Tələbə nömrəsi 30 simvoldan çox ola bilməz.")]
        public string StudentNumber { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Düzgün telefon nömrəsi daxil edin.")]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Akademik il mütləqdir.")]
        [Range(1, 6, ErrorMessage = "Akademik il 1 ilə 6 arasında olmalıdır.")]
        public int AcademicYear { get; set; } = 1;

        [Required(ErrorMessage = "Təhsil proqramı seçilməlidir.")]
        [Range(1, int.MaxValue, ErrorMessage = "Düzgün təhsil proqramı seçin.")]
        public int ProgrammeId { get; set; }
    }
}
