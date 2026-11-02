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

        #region F5: Davamiyyət İdarəsi (Teacher)

        // Müəllimin tədris etdiyi fənlər üzrə davamiyyət paneli
        [HttpGet]
        public async Task<IActionResult> Attendance()
        {
            var courses = await _apiService.GetAsync<List<CourseViewModel>>("api/courses/my-courses") ?? new List<CourseViewModel>();
            return View(courses);
        }

        // Fənn üzrə keçirilmiş dərslərin siyahısı
        [HttpGet]
        public async Task<IActionResult> AttendanceSessions(int courseId)
        {
            var course = await _apiService.GetAsync<CourseViewModel>($"api/courses/{courseId}");
            if (course == null)
            {
                TempData["ErrorMessage"] = "Kurs tapılmadı.";
                return RedirectToAction(nameof(Attendance));
            }

            var sessions = await _apiService.GetAsync<List<LessonSessionSummaryViewModel>>($"api/attendance/course/{courseId}/sessions")
                ?? new List<LessonSessionSummaryViewModel>();

            var model = new TeacherCourseAttendanceOverviewViewModel
            {
                Course = course,
                Sessions = sessions
            };

            return View(model);
        }

        // Yeni dərs sessiyası yaratmaq üçün forma
        [HttpGet]
        public async Task<IActionResult> CreateAttendanceSession(int courseId)
        {
            var template = await _apiService.GetAsync<LessonSessionFormViewModel>($"api/attendance/course/{courseId}/template");
            if (template == null)
            {
                TempData["ErrorMessage"] = "Dərs şablonu alına bilmədi. Bu fənn sizə təhkim olunmaya bilər və ya fəndə hələ tələbə yoxdur.";
                return RedirectToAction(nameof(AttendanceSessions), new { courseId });
            }

            template.LessonDate = DateTime.Today;
            return View("AttendanceForm", template);
        }

        // Yeni sessiyanı yadda saxlamaq
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAttendanceSession(LessonSessionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("AttendanceForm", model);
            }

            var payload = new
            {
                CourseId = model.CourseId,
                LessonDate = model.LessonDate,
                LessonTopic = model.LessonTopic,
                Records = model.Records.Select(r => new
                {
                    StudentId = r.StudentId,
                    Status = r.Status,
                    Note = r.Note
                }).ToList()
            };

            var (success, _, error) = await _apiService.PostWithResultAsync<object, object>("api/attendance/session", payload);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Davamiyyət saxlanılarkən xəta baş verdi.");
                return View("AttendanceForm", model);
            }

            TempData["SuccessMessage"] = $"'{model.LessonTopic}' dərsinin davamiyyəti uğurla qeydə alındı.";
            return RedirectToAction(nameof(AttendanceSessions), new { courseId = model.CourseId });
        }

        // Əvvəlki sessiyanı redaktə etmək
        [HttpGet]
        public async Task<IActionResult> EditAttendanceSession(int courseId, DateTime date)
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var session = await _apiService.GetAsync<LessonSessionFormViewModel>($"api/attendance/course/{courseId}/session?date={dateStr}");

            if (session == null)
            {
                TempData["ErrorMessage"] = "Dərs sessiyası tapılmadı.";
                return RedirectToAction(nameof(AttendanceSessions), new { courseId });
            }

            session.OriginalLessonDate = date;
            return View("AttendanceForm", session);
        }

        // Redaktə olunmuş sessiyanı yadda saxlamaq
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAttendanceSession(LessonSessionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("AttendanceForm", model);
            }

            var payload = new
            {
                CourseId = model.CourseId,
                OriginalLessonDate = model.OriginalLessonDate ?? model.LessonDate,
                LessonDate = model.LessonDate,
                LessonTopic = model.LessonTopic,
                Records = model.Records.Select(r => new
                {
                    StudentId = r.StudentId,
                    Status = r.Status,
                    Note = r.Note
                }).ToList()
            };

            var (success, error) = await _apiService.PutWithResultAsync("api/attendance/session", payload);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Davamiyyət yenilənərkən xəta baş verdi.");
                return View("AttendanceForm", model);
            }

            TempData["SuccessMessage"] = "Dərs sessiyasının davamiyyəti uğurla yeniləndi.";
            return RedirectToAction(nameof(AttendanceSessions), new { courseId = model.CourseId });
        }

        // Sessiyanı silmək
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttendanceSession(int courseId, DateTime date)
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var (success, error) = await _apiService.DeleteWithResultAsync($"api/attendance/course/{courseId}/session?date={dateStr}");

            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Sessiya silinə bilmədi.";
            }
            else
            {
                TempData["SuccessMessage"] = "Dərs sessiyası uğurla silindi.";
            }

            return RedirectToAction(nameof(AttendanceSessions), new { courseId });
        }

        #endregion
    }
}
