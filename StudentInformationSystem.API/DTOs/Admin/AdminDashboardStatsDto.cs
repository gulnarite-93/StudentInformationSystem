namespace StudentInformationSystem.API.DTOs.Admin
{
    public class AdminDashboardStatsDto
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalDepartments { get; set; }
        public decimal AverageGpa { get; set; }
        public List<DepartmentStudentCountDto> DepartmentStudentCounts { get; set; } = new();
    }

    public class DepartmentStudentCountDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;
        public int StudentCount { get; set; }
    }
}
