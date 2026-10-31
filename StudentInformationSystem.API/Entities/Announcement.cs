namespace StudentInformationSystem.API.Entities
{
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;

        // Əgər CourseId null-dırsa, bu bütün müəssisə üzrə (ümumi) elandır.
        // Əgər CourseId varsa, həmin kursa aid daxili elandır.
        public int? CourseId { get; set; }
        public Course? Course { get; set; }
    }
}
