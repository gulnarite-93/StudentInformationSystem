using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.Web.Models;
using StudentInformationSystem.Web.Services;

namespace StudentInformationSystem.Web.Controllers
{
    [Authorize(Roles = "Teacher,Admin")]
    public class TeacherController : Controller
    {
        private readonly IApiService _apiService;

        public TeacherController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // F4: Müəllimin tədris etdiyi fənlər və qiymətləndirmə siyahısı
        [HttpGet]
        public async Task<IActionResult> Grades()
        {
            var courses = await _apiService.GetAsync<List<TeacherGradeCourseSummaryViewModel>>("api/grades/teacher-courses") 
                          ?? new List<TeacherGradeCourseSummaryViewModel>();

            return View(courses);
        }

        // F4: Kurs üzrə tələbələrin qiymət cədvəli (baxış və daxil etmə)
        [HttpGet]
        public async Task<IActionResult> CourseGrades(int courseId)
        {
            var courseGrades = await _apiService.GetAsync<CourseGradesViewModel>($"api/grades/course/{courseId}");
            if (courseGrades == null)
            {
                TempData["ErrorMessage"] = "Fənn tapılmadı və ya bu fənn sizə təhkim olunmayıb.";
                return RedirectToAction(nameof(Grades));
            }

            return View(courseGrades);
        }

        // F4: Tək tələbənin qiymətini saxlamaq/redaktə etmək
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrade(GradeSaveInputModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Məlumatlar düzgün daxil edilməyib. Zəhmət olmasa daxil edilən balları yoxlayın (0 - 100 aralığında olmalıdır).";
                return RedirectToAction(nameof(CourseGrades), new { courseId = model.CourseId });
            }

            var payload = new
            {
                EnrollmentId = model.EnrollmentId,
                MidtermScore = model.MidtermScore,
                AssignmentScore = model.AssignmentScore,
                FinalScore = model.FinalScore,
                TotalScore = model.TotalScore,
                Feedback = model.Feedback
            };

            var (success, _, error) = await _apiService.PostWithResultAsync<object, object>("api/grades", payload);
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Qiymət saxlanılarkən xəta baş verdi.";
            }
            else
            {
                var nameText = !string.IsNullOrEmpty(model.StudentName) ? $"({model.StudentName})" : "";
                TempData["SuccessMessage"] = $"Tələbənin {nameText} qiyməti uğurla yadda saxlanıldı və GPA avtomatik yeniləndi.";
            }

            return RedirectToAction(nameof(CourseGrades), new { courseId = model.CourseId });
        }
    }
}
