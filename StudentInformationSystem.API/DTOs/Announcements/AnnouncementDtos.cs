using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.API.DTOs.Announcements
{
    public class AnnouncementDto
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

        /// <summary>
        /// Kurs ID null olduqda bütün müəssisə üzrə (ümumi) elandır.
        /// </summary>
        public bool IsInstitutionWide => !CourseId.HasValue;

        /// <summary>
        /// Hazırkı istifadəçinin bu elanı redaktə etmək hüququ olub-olmaması
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Hazırkı istifadəçinin bu elanı silmək hüququ olub-olmaması
        /// </summary>
        public bool CanDelete { get; set; }
    }

    public class AnnouncementCreateDto
    {
        [Required(ErrorMessage = "Elanın başlığı mütləq daxil edilməlidir.")]
        [StringLength(200, ErrorMessage = "Başlıq 200 simvoldan çox ola bilməz.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Elanın mətni mütləq daxil edilməlidir.")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// null olduqda müəssisə üzrə ümumi elan sayılır (yalnız Admin tərəfindən yaradıla bilər).
        /// Dəyər olduqda yalnız həmin kursa aid daxili elan sayılır.
        /// </summary>
        public int? CourseId { get; set; }
    }

    public class AnnouncementUpdateDto
    {
        [Required(ErrorMessage = "Elanın başlığı mütləq daxil edilməlidir.")]
        [StringLength(200, ErrorMessage = "Başlıq 200 simvoldan çox ola bilməz.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Elanın mətni mütləq daxil edilməlidir.")]
        public string Content { get; set; } = string.Empty;

        public int? CourseId { get; set; }
    }

    public class AnnouncementCourseOptionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
