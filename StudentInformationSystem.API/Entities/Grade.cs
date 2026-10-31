namespace StudentInformationSystem.API.Entities
{
    public class Grade
    {
        public int Id { get; set; }

        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;

        public decimal? MidtermScore { get; set; } // Məs: 0 - 50 və ya 0 - 100
        public decimal? FinalScore { get; set; }   // Məs: 0 - 50 və ya 0 - 100
        public decimal TotalScore { get; set; }    // 0 - 100
        public string LetterGrade { get; set; } = "F"; // A, B, C, D, E, F
        public decimal GradePoint { get; set; } = 0.0m; // 4.0, 3.5, 3.0, 2.0, 1.0, 0.0 (GPA üçün)
        public string? Feedback { get; set; }
        public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    }
}
