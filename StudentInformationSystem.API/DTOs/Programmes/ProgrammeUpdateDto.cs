using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.API.DTOs.Programmes
{
    public class ProgrammeUpdateDto
    {
        [Required(ErrorMessage = "Proqram adı mütləqdir.")]
        [StringLength(100, ErrorMessage = "Proqram adı 100 simvoldan çox ola bilməz.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Proqram kodu mütləqdir.")]
        [StringLength(20, ErrorMessage = "Proqram kodu 20 simvoldan çox ola bilməz.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şöbə seçilməlidir.")]
        [Range(1, int.MaxValue, ErrorMessage = "Zəhmət olmasa düzgün şöbə seçin.")]
        public int DepartmentId { get; set; }
    }
}
