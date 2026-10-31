namespace StudentInformationSystem.API.DTOs.Terms
{
    public class TermDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int CourseCount { get; set; }
    }
}
