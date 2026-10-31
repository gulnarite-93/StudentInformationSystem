using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.API.DTOs.Terms
{
    public class TermCreateDto
    {
        [Required(ErrorMessage = "Akademik dövrün adı mütləqdir.")]
        [StringLength(100, ErrorMessage = "Dövr adı 100 simvoldan çox ola bilməz.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Başlanğıc tarixi mütləqdir.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitmə tarixi mütləqdir.")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
