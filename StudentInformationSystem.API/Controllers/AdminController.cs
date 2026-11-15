using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.Data;
using StudentInformationSystem.API.DTOs.Admin;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IGpaService _gpaService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            AppDbContext context,
            IGpaService gpaService,
            ILogger<AdminController> logger)
        {
            _context = context;
            _gpaService = gpaService;
            _logger = logger;
        }

        [HttpGet("dashboard-stats")]
        public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                var totalStudents = await _context.Users.CountAsync(u => u.Role == UserRoles.Student);
                var totalTeachers = await _context.Users.CountAsync(u => u.Role == UserRoles.Teacher);
                var totalCourses = await _context.Courses.CountAsync();
                var totalDepartments = await _context.Departments.CountAsync();

                // 1. Şöbələr üzrə tələbə sayları
                var departments = await _context.Departments
                    .AsNoTracking()
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                var studentsByDept = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Role == UserRoles.Student && u.Programme != null)
                    .GroupBy(u => u.Programme!.DepartmentId)
                    .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.DepartmentId, x => x.Count);

                var deptCounts = departments.Select(d => new DepartmentStudentCountDto
                {
                    DepartmentId = d.Id,
                    DepartmentName = d.Name,
                    DepartmentCode = d.Code,
                    StudentCount = studentsByDept.TryGetValue(d.Id, out var count) ? count : 0
                }).ToList();

                var unassignedCount = await _context.Users
                    .CountAsync(u => u.Role == UserRoles.Student && (u.ProgrammeId == null || u.Programme == null));

                if (unassignedCount > 0)
                {
                    deptCounts.Add(new DepartmentStudentCountDto
                    {
                        DepartmentId = 0,
                        DepartmentName = "Təyin olunmamış",
                        DepartmentCode = "N/A",
                        StudentCount = unassignedCount
                    });
                }

                // 2. Bütün tələbələrin orta GPA-sı
                var gradedEnrollments = await _context.Enrollments
                    .AsNoTracking()
                    .Include(e => e.Grade)
                    .Include(e => e.Course)
                    .Where(e => e.Grade != null && (e.Status == "Enrolled" || e.Status == "Completed") && e.Student.Role == UserRoles.Student)
                    .Select(e => new
                    {
                        StudentId = e.StudentId,
                        GradePoint = e.Grade!.GradePoint,
                        Credits = e.Course.Credits
                    })
                    .ToListAsync();

                var studentGroups = gradedEnrollments.GroupBy(e => e.StudentId);
                var studentGpas = new List<decimal>();

                foreach (var group in studentGroups)
                {
                    var gpa = _gpaService.CalculateGpa(group.Select(g => (g.GradePoint, g.Credits)));
                    studentGpas.Add(gpa);
                }

                decimal averageGpa = studentGpas.Any()
                    ? Math.Round(studentGpas.Average(), 2, MidpointRounding.AwayFromZero)
                    : 0.00m;

                var result = new AdminDashboardStatsDto
                {
                    TotalStudents = totalStudents,
                    TotalTeachers = totalTeachers,
                    TotalCourses = totalCourses,
                    TotalDepartments = totalDepartments,
                    AverageGpa = averageGpa,
                    DepartmentStudentCounts = deptCounts
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin statistika məlumatları toplanarkən xəta baş verdi.");
                return StatusCode(500, new { message = $"Statistika məlumatları alına bilmədi: {ex.Message}" });
            }
        }
    }
}
