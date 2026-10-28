namespace StudentInformationSystem.API.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // Məs: CS101
        public string Name { get; set; } = string.Empty; // Məs: Alqoritmlər və Verilənlər Strukturu
        public int Credits { get; set; } = 3; // F2: Kredit
        public string? Description { get; set; }

        public int ProgrammeId { get; set; }
        public Programme Programme { get; set; } = null!;

        public int TermId { get; set; }
        public Term Term { get; set; } = null!;

        // F2: Müəllim təyinatı
        public int? TeacherId { get; set; }
        public User? Teacher { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    }
}
