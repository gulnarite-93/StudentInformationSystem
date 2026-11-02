namespace StudentInformationSystem.API.Entities
{
    public class Attendance
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public int StudentId { get; set; }
        public User Student { get; set; } = null!;

        public DateTime LessonDate { get; set; }
        public string? LessonTopic { get; set; } // Dərsin mövzusu
        public bool IsPresent { get; set; } = true; // İştirak edib ya yox
        public string Status { get; set; } = AttendanceStatus.Present; // Present (Gəldi), Absent (Gəlmədi), Excused (İcazəli)
        public string? Note { get; set; } // Qeyd
    }
}
