using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.API.DTOs.Departments
{
    public class DepartmentUpdateDto
    {
        [Required(ErrorMessage = "Şöbə adı mütləqdir.")]
        [StringLength(100, ErrorMessage = "Şöbə adı 100 simvoldan çox ola bilməz.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şöbə kodu mütləqdir.")]
        [StringLength(20, ErrorMessage = "Şöbə kodu 20 simvoldan çox ola bilməz.")]
        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
