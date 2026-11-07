using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentInformationSystem.Web.Models
{
    public class CourseViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? Description { get; set; }

        public int ProgrammeId { get; set; }
        public string ProgrammeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;

        public int TermId { get; set; }
        public string TermName { get; set; } = string.Empty;
        public bool IsTermActive { get; set; }

        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public string? TeacherTitle { get; set; }

        public int EnrolledStudentsCount { get; set; }
    }

    public class CourseFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kurs kodu mütləqdir.")]
        [Display(Name = "Kurs Kodu")]
        [StringLength(20, ErrorMessage = "Kurs kodu maksimum 20 simvol ola bilər.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kurs adı mütləqdir.")]
        [Display(Name = "Kursun Adı")]
        [StringLength(150, ErrorMessage = "Kurs adı maksimum 150 simvol ola bilər.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kredit sayı mütləqdir.")]
        [Display(Name = "Kredit Sayı")]
        [Range(1, 30, ErrorMessage = "Kredit 1 ilə 30 arasında olmalıdır.")]
        public int Credits { get; set; } = 3;

        [Display(Name = "Təsvir")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Təhsil proqramı seçilməlidir.")]
        [Display(Name = "Təhsil Proqramı")]
        [Range(1, int.MaxValue, ErrorMessage = "Zəhmət olmasa proqram seçin.")]
        public int ProgrammeId { get; set; }

        [Required(ErrorMessage = "Akademik dövr (semestr) seçilməlidir.")]
        [Display(Name = "Akademik Dövr (Semestr)")]
        [Range(1, int.MaxValue, ErrorMessage = "Zəhmət olmasa semestr seçin.")]
        public int TermId { get; set; }

        [Display(Name = "Təhkim Olunmuş Müəllim")]
        public int? TeacherId { get; set; }

        public IEnumerable<SelectListItem> ProgrammeList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TermList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TeacherList { get; set; } = new List<SelectListItem>();
    }

    public class AssignTeacherViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? CurrentTeacherName { get; set; }

        [Display(Name = "Yeni Müəllim Seçin")]
        public int? TeacherId { get; set; }

        public IEnumerable<SelectListItem> TeacherList { get; set; } = new List<SelectListItem>();
    }

    public class TeacherSelectItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? DepartmentName { get; set; }
    }
}
