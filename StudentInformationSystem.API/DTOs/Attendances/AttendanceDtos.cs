using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.API.DTOs.Attendances
{
    public class AttendanceRecordDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public string Status { get; set; } = "Present"; // Present, Absent, Excused
        public string? Note { get; set; }
    }

    public class LessonSessionSummaryDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime LessonDate { get; set; }
        public string? LessonTopic { get; set; }
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int ExcusedCount { get; set; }
    }

    public class LessonSessionDetailsDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime LessonDate { get; set; }
        public string? LessonTopic { get; set; }
        public List<AttendanceRecordDto> Records { get; set; } = new();
    }

    public class AttendanceStudentInputDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public string Status { get; set; } = "Present"; // Present, Absent, Excused

        public string? Note { get; set; }
    }

    public class CreateLessonSessionDto
    {
        [Required(ErrorMessage = "Kurs mütləq seçilməlidir.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Dərs tarixi daxil edilməlidir.")]
        public DateTime LessonDate { get; set; } = DateTime.UtcNow.Date;

        [Required(ErrorMessage = "Dərsin mövzusu daxil edilməlidir.")]
        [StringLength(200, ErrorMessage = "Mövzu 200 simvoldan çox ola bilməz.")]
        public string LessonTopic { get; set; } = string.Empty;

        public List<AttendanceStudentInputDto> Records { get; set; } = new();
    }

    public class UpdateLessonSessionDto
    {
        [Required(ErrorMessage = "Kurs mütləq seçilməlidir.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Orijinal dərs tarixi tələb olunur.")]
        public DateTime OriginalLessonDate { get; set; }

        [Required(ErrorMessage = "Dərs tarixi daxil edilməlidir.")]
        public DateTime LessonDate { get; set; }

        [Required(ErrorMessage = "Dərsin mövzusu daxil edilməlidir.")]
        [StringLength(200, ErrorMessage = "Mövzu 200 simvoldan çox ola bilməz.")]
        public string LessonTopic { get; set; } = string.Empty;

        public List<AttendanceStudentInputDto> Records { get; set; } = new();
    }

    public class StudentAttendanceItemDto
    {
        public int Id { get; set; }
        public DateTime LessonDate { get; set; }
        public string? LessonTopic { get; set; }
        public string Status { get; set; } = "Present"; // Present, Absent, Excused
        public string? Note { get; set; }
    }

    public class StudentCourseAttendanceSummaryDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? TeacherName { get; set; }
        public string TermName { get; set; } = string.Empty;
        public int TotalLessons { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int ExcusedCount { get; set; }
        public double AttendancePercentage { get; set; }
        public List<StudentAttendanceItemDto> History { get; set; } = new();
    }
}
