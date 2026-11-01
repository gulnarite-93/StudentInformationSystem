using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.Web.Models;
using StudentInformationSystem.Web.Services;

namespace StudentInformationSystem.Web.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly IApiService _apiService;

        public TeacherController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            var courses = await _apiService.GetAsync<List<CourseViewModel>>("api/courses/my-courses") ?? new List<CourseViewModel>();
            return View(courses);
        }

        // F3: Müəllimin öz kursuna yazılmış tələbələrin siyahısı
        [HttpGet]
        public async Task<IActionResult> CourseStudents(int courseId)
        {
            var course = await _apiService.GetAsync<CourseViewModel>($"api/courses/{courseId}");
            if (course == null)
            {
                TempData["ErrorMessage"] = "Kurs tapılmadı.";
                return RedirectToAction(nameof(Courses));
            }

            var students = await _apiService.GetAsync<List<EnrolledStudentViewModel>>($"api/enrollments/course/{courseId}") 
                ?? new List<EnrolledStudentViewModel>();

            var model = new CourseStudentsViewModel
            {
                Course = course,
                Students = students
            };

            return View(model);
        }
    }
}
