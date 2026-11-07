using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentInformationSystem.Web.Models
{
    public class AnnouncementItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorRole { get; set; } = string.Empty;
        public int? CourseId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public bool IsInstitutionWide { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class AnnouncementFormViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Elanın başlığını daxil edin")]
        [StringLength(200, ErrorMessage = "Başlıq maksimum 200 simvol ola bilər")]
        [Display(Name = "Başlıq")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Elanın mətnini daxil edin")]
        [Display(Name = "Məzmun")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Hədəf Auditoriya / Kurs")]
        public int? CourseId { get; set; }

        public List<SelectListItem> CourseOptions { get; set; } = new();

        public bool IsAdmin { get; set; }
        public bool IsTeacher { get; set; }
    }

    public class AnnouncementCourseOptionViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
