namespace StudentInformationSystem.API.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = UserRoles.Student; // Admin, Teacher, Student
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // F1: Tələbə profili xüsusiyyətləri
        public string? StudentNumber { get; set; } // Məs: ST2026001
        public int? AcademicYear { get; set; } // 1, 2, 3, 4-cü kurs
        public int? ProgrammeId { get; set; }
        public Programme? Programme { get; set; }

        // Müəllim xüsusiyyətləri
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public string? Title { get; set; } // Məs: Dosent, Baş müəllim

        // Əlaqələr
        public ICollection<Course> TaughtCourses { get; set; } = new List<Course>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Announcement> AuthoredAnnouncements { get; set; } = new List<Announcement>();
    }
}
