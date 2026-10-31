using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.Web.Models
{
    public class TermViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int CourseCount { get; set; }
    }

    public class TermFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Akademik dövrün adı mütləq daxil edilməlidir.")]
        [Display(Name = "Dövrün Adı")]
        [StringLength(100, ErrorMessage = "Dövrün adı maksimum 100 simvol ola bilər.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Başlanğıc tarixi mütləq daxil edilməlidir.")]
        [Display(Name = "Başlanğıc Tarixi")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Bitmə tarixi mütləq daxil edilməlidir.")]
        [Display(Name = "Bitmə Tarixi")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(4);

        [Display(Name = "Aktiv Dövrdür?")]
        public bool IsActive { get; set; } = true;
    }
}
