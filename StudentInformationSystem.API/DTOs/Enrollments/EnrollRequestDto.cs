using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.API.DTOs.Enrollments
{
    public class EnrollRequestDto
    {
        [Required(ErrorMessage = "Kurs seçilməlidir.")]
        public int CourseId { get; set; }

        // Yalnız Admin üçün (tələbə göndərərsə, tələbənin öz ID-si ilə əvəzlənir)
        public int? StudentId { get; set; }
    }

    public class DropRequestDto
    {
        [Required(ErrorMessage = "Kurs seçilməlidir.")]
        public int CourseId { get; set; }

        public int? StudentId { get; set; }
    }
}
