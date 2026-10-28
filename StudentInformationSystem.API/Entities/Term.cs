namespace StudentInformationSystem.API.Entities
{
    public class Term
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Məs: 2026-2027 Payız Semestri
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
