using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.Web.Models;
using StudentInformationSystem.Web.Services;

namespace StudentInformationSystem.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IApiService _apiService;

        public StudentController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // F6: Akademik Panel (Dashboard)
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _apiService.GetAsync<StudentDashboardViewModel>("api/grades/dashboard");
            if (dashboard == null)
            {
                TempData["ErrorMessage"] = "Akademik panel məlumatları alına bilmədi.";
                return View(new StudentDashboardViewModel());
            }

            return View(dashboard);
        }

        // F4: Qiymətlərim səhifəsi
        [HttpGet]
        public async Task<IActionResult> Grades()
        {
            var grades = await _apiService.GetAsync<List<StudentGradeItemViewModel>>("api/grades/my-grades")
                         ?? new List<StudentGradeItemViewModel>();

            return View(grades);
        }

        // F4 & F6: Rəsmi Transkript səhifəsi
        [HttpGet]
        public async Task<IActionResult> Transcript()
        {
            var transcript = await _apiService.GetAsync<TranscriptViewModel>("api/grades/transcript");
            if (transcript == null)
            {
                TempData["ErrorMessage"] = "Transkript alına bilmədi.";
                return View(new TranscriptViewModel());
            }

            return View(transcript);
        }
    }
}
