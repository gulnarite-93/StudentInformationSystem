namespace StudentInformationSystem.API.Entities
{
    public class Enrollment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public User Student { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Enrolled"; // Enrolled, Completed, Dropped

        // F4: Qiymət məlumatı
        public Grade? Grade { get; set; }
    }
}
