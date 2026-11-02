using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.Web.Models
{
    public class AttendanceRecordViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentNumber { get; set; }
        public string Status { get; set; } = "Present"; // Present (Gəldi), Absent (Gəlmədi), Excused (İcazəli)
        public string? Note { get; set; }
    }

    public class LessonSessionSummaryViewModel
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

    public class LessonSessionFormViewModel
    {
        [Required(ErrorMessage = "Kurs mütləq seçilməlidir.")]
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        public DateTime? OriginalLessonDate { get; set; }

        [Required(ErrorMessage = "Dərs tarixi daxil edilməlidir.")]
        [DataType(DataType.Date)]
        public DateTime LessonDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Dərsin mövzusu daxil edilməlidir.")]
        [StringLength(200, ErrorMessage = "Mövzu 200 simvoldan çox ola bilməz.")]
        public string LessonTopic { get; set; } = string.Empty;

        public List<AttendanceRecordViewModel> Records { get; set; } = new();

        public bool IsEditMode => OriginalLessonDate.HasValue;
    }

    public class TeacherCourseAttendanceOverviewViewModel
    {
        public CourseViewModel Course { get; set; } = new();
        public List<LessonSessionSummaryViewModel> Sessions { get; set; } = new();
    }

    public class StudentAttendanceItemViewModel
    {
        public int Id { get; set; }
        public DateTime LessonDate { get; set; }
        public string? LessonTopic { get; set; }
        public string Status { get; set; } = "Present";
        public string? Note { get; set; }
    }

    public class StudentCourseAttendanceSummaryViewModel
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
        public List<StudentAttendanceItemViewModel> History { get; set; } = new();
    }

    public class StudentAttendancePageViewModel
    {
        public List<StudentCourseAttendanceSummaryViewModel> Courses { get; set; } = new();
        public int OverallTotalLessons => Courses.Sum(c => c.TotalLessons);
        public int OverallPresentCount => Courses.Sum(c => c.PresentCount);
        public int OverallAbsentCount => Courses.Sum(c => c.AbsentCount);
        public int OverallExcusedCount => Courses.Sum(c => c.ExcusedCount);
        public double OverallPercentage => OverallTotalLessons > 0 ? Math.Round((double)OverallPresentCount / OverallTotalLessons * 100, 1) : 100.0;
    }
}
