namespace StudentInformationSystem.API.Entities
{
    public class Programme
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Məs: İnformasiya Texnologiyaları
        public string Code { get; set; } = string.Empty; // Məs: IT
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public ICollection<User> Students { get; set; } = new List<User>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
