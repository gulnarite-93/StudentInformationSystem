namespace StudentInformationSystem.API.DTOs.Programmes
{
    public class ProgrammeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int CourseCount { get; set; }
    }
}
