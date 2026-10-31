using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentInformationSystem.Web.Models
{
    public class ProgrammeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int CourseCount { get; set; }
    }

    public class ProgrammeFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Proqram adı mütləq daxil edilməlidir.")]
        [Display(Name = "Proqramın Adı")]
        [StringLength(100, ErrorMessage = "Proqram adı maksimum 100 simvol ola bilər.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Proqram kodu mütləq daxil edilməlidir.")]
        [Display(Name = "Proqram Kodu")]
        [StringLength(20, ErrorMessage = "Proqram kodu maksimum 20 simvol ola bilər.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aid olduğu şöbə mütləq seçilməlidir.")]
        [Display(Name = "Şöbə")]
        [Range(1, int.MaxValue, ErrorMessage = "Zəhmət olmasa siyahıdan bir şöbə seçin.")]
        public int DepartmentId { get; set; }

        public IEnumerable<SelectListItem> DepartmentList { get; set; } = new List<SelectListItem>();
    }
}
