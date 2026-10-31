using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.Web.Models
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProgrammeCount { get; set; }
        public int TeacherCount { get; set; }
    }

    public class DepartmentFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şöbə adı mütləq daxil edilməlidir.")]
        [Display(Name = "Şöbənin Adı")]
        [StringLength(100, ErrorMessage = "Şöbə adı maksimum 100 simvol ola bilər.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şöbə kodu mütləq daxil edilməlidir.")]
        [Display(Name = "Şöbə Kodu")]
        [StringLength(20, ErrorMessage = "Şöbə kodu maksimum 20 simvol ola bilər.")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Təsvir")]
        public string? Description { get; set; }
    }
}
