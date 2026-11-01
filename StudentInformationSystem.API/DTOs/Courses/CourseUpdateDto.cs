using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.API.DTOs.Courses
{
    public class CourseUpdateDto
    {
        [Required(ErrorMessage = "Kurs kodu mütləqdir.")]
        [StringLength(20, ErrorMessage = "Kurs kodu 20 simvoldan çox ola bilməz.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kurs adı mütləqdir.")]
        [StringLength(150, ErrorMessage = "Kurs adı 150 simvoldan çox ola bilməz.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kredit sayı mütləqdir.")]
        [Range(1, 30, ErrorMessage = "Kredit 1 ilə 30 arasında olmalıdır.")]
        public int Credits { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Təhsil proqramı seçilməlidir.")]
        [Range(1, int.MaxValue, ErrorMessage = "Düzgün proqram seçin.")]
        public int ProgrammeId { get; set; }

        [Required(ErrorMessage = "Akademik dövr (semestr) seçilməlidir.")]
        [Range(1, int.MaxValue, ErrorMessage = "Düzgün semestr seçin.")]
        public int TermId { get; set; }

        public int? TeacherId { get; set; }
    }
}
