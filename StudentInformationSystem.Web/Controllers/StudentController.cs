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

        #region Akademik Panel və Profil

        // Akademik Panel (Dashboard)
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

        // Şəxsi Tələbə Profili
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var profile = await _apiService.GetAsync<StudentProfileViewModel>("api/students/profile");
            if (profile == null)
            {
                TempData["ErrorMessage"] = "Profil məlumatları alına bilmədi.";
                return RedirectToAction("Index", "Home");
            }

            return View(profile);
        }

        #endregion

        #region Kurs Seçimi və Qeydiyyat (Enrollment)

        // Kurs Seçimi və Qeydiyyat Səhifəsi
        [HttpGet]
        public async Task<IActionResult> Enrollment()
        {
            var myEnrollments = await _apiService.GetAsync<List<EnrollmentViewModel>>("api/enrollments/my") 
                ?? new List<EnrollmentViewModel>();

            var availableCourses = await _apiService.GetAsync<List<StudentCourseRegistrationItemViewModel>>("api/enrollments/available") 
                ?? new List<StudentCourseRegistrationItemViewModel>();

            var model = new StudentCourseRegistrationPageViewModel
            {
                MyEnrollments = myEnrollments,
                AvailableCourses = availableCourses
            };

            return View(model);
        }

        // Kursa Yazılma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var (success, _, errorMessage) = await _apiService.PostWithResultAsync<object, EnrollmentViewModel>(
                "api/enrollments/enroll", 
                new { CourseId = courseId });

            if (success)
            {
                TempData["SuccessMessage"] = "Kursa uğurla yazıldınız!";
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage ?? "Kursa qeydiyyat zamanı xəta baş verdi.";
            }

            return RedirectToAction(nameof(Enrollment));
        }

        // Kursdan Çıxma (Drop) - enrollmentId vasitəsilə
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Drop(int enrollmentId)
        {
            var (success, _, errorMessage) = await _apiService.PostWithResultAsync<object, object>(
                $"api/enrollments/{enrollmentId}/drop", 
                new { });

            if (success)
            {
                TempData["SuccessMessage"] = "Kursdan uğurla imtina etdiniz (çıxarıldınız).";
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage ?? "Kursdan çıxarkən xəta baş verdi.";
            }

            return RedirectToAction(nameof(Enrollment));
        }

        // Kursdan Çıxma (Drop) - courseId vasitəsilə
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task DropCourse(int courseId)
        {
            var (success, _, errorMessage) = await _apiService.PostWithResultAsync<object, object>(
                "api/enrollments/drop-course", 
                new { CourseId = courseId });

            if (success)
            {
                TempData["SuccessMessage"] = "Kursdan uğurla imtina etdiniz.";
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage ?? "Kursdan çıxarkən xəta baş verdi.";
            }

            Response.Redirect(Url.Action(nameof(Enrollment))!);
        }

        #endregion

        #region Davamiyyət

        // Tələbənin Davamiyyət Səhifəsi
        [HttpGet]
        public async Task<IActionResult> Attendance()
        {
            var summaries = await _apiService.GetAsync<List<StudentCourseAttendanceSummaryViewModel>>("api/attendance/my") 
                ?? new List<StudentCourseAttendanceSummaryViewModel>();

            var model = new StudentAttendancePageViewModel
            {
                Courses = summaries
            };

            return View(model);
        }

        #endregion

        #region Qiymətlər və Transkript

        // Qiymətlərim səhifəsi
        [HttpGet]
        public async Task<IActionResult> Grades()
        {
            var grades = await _apiService.GetAsync<List<StudentGradeItemViewModel>>("api/grades/my-grades")
                         ?? new List<StudentGradeItemViewModel>();

            return View(grades);
        }

        // Rəsmi Transkript səhifəsi
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

        #endregion
    }
}
