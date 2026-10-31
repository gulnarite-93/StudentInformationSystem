namespace StudentInformationSystem.API.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Məs: Kompüter Elmləri
        public string Code { get; set; } = string.Empty; // Məs: CS
        public string? Description { get; set; }

        public ICollection<Programme> Programmes { get; set; } = new List<Programme>();
        public ICollection<User> Teachers { get; set; } = new List<User>();
    }
}
