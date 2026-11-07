using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentInformationSystem.Web.Models
{
    public class StudentViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? StudentNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? AcademicYear { get; set; }
        public int? ProgrammeId { get; set; }
        public string? ProgrammeName { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StudentFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "İstifadəçi adı mütləqdir.")]
        [Display(Name = "İstifadəçi Adı")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "İstifadəçi adı 3-50 simvol aralığında olmalıdır.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-poçt ünvanı mütləqdir.")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt formatı daxil edin.")]
        [Display(Name = "E-poçt")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Şifrə")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifrə ən azı 6 simvol olmalıdır.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Ad mütləqdir.")]
        [Display(Name = "Ad")]
        [StringLength(50, ErrorMessage = "Ad 50 simvoldan çox ola bilməz.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad mütləqdir.")]
        [Display(Name = "Soyad")]
        [StringLength(50, ErrorMessage = "Soyad 50 simvoldan çox ola bilməz.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tələbə nömrəsi mütləqdir.")]
        [Display(Name = "Tələbə Nömrəsi (ST ID)")]
        [StringLength(30, ErrorMessage = "Tələbə nömrəsi 30 simvoldan çox ola bilməz.")]
        public string StudentNumber { get; set; } = string.Empty;

        [Display(Name = "Əlaqə Telefonu")]
        [Phone(ErrorMessage = "Düzgün telefon nömrəsi daxil edin.")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Doğum Tarixi")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Akademik il mütləqdir.")]
        [Display(Name = "Akademik İl (Kurs)")]
        [Range(1, 6, ErrorMessage = "Akademik il 1 ilə 6 arasında olmalıdır.")]
        public int AcademicYear { get; set; } = 1;

        [Required(ErrorMessage = "Təhsil proqramı seçilməlidir.")]
        [Display(Name = "Təhsil Proqramı")]
        [Range(1, int.MaxValue, ErrorMessage = "Zəhmət olmasa təhsil proqramı seçin.")]
        public int ProgrammeId { get; set; }

        public IEnumerable<SelectListItem> ProgrammeList { get; set; } = new List<SelectListItem>();
    }

    public class StudentProfileViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? StudentNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? AcademicYear { get; set; }
        public int? ProgrammeId { get; set; }
        public string? ProgrammeName { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EnrolledCoursesCount { get; set; }
        public int CompletedCoursesCount { get; set; }
    }
}
