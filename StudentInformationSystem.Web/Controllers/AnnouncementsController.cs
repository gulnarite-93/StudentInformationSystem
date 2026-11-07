using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentInformationSystem.Web.Models;
using StudentInformationSystem.Web.Services;

namespace StudentInformationSystem.Web.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly IApiService _apiService;

        public AnnouncementsController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Announcements
        [HttpGet]
        public async Task<IActionResult> Index(string? filter = null)
        {
            var announcements = await _apiService.GetAsync<List<AnnouncementItemViewModel>>("api/announcements")
                                ?? new List<AnnouncementItemViewModel>();

            if (filter == "institution")
            {
                announcements = announcements.Where(a => a.IsInstitutionWide).ToList();
            }
            else if (filter == "course")
            {
                announcements = announcements.Where(a => !a.IsInstitutionWide).ToList();
            }

            ViewBag.CurrentFilter = filter;
            return View(announcements);
        }

        // GET: /Announcements/Create
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create()
        {
            var model = new AnnouncementFormViewModel
            {
                IsAdmin = User.IsInRole("Admin"),
                IsTeacher = User.IsInRole("Teacher")
            };

            await LoadCourseOptionsAsync(model);
            return View(model);
        }

        // POST: /Announcements/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementFormViewModel model)
        {
            model.IsAdmin = User.IsInRole("Admin");
            model.IsTeacher = User.IsInRole("Teacher");

            // Müəllimlər mütləq kurs seçməlidir
            if (model.IsTeacher && !model.IsAdmin && (!model.CourseId.HasValue || model.CourseId.Value <= 0))
            {
                ModelState.AddModelError(nameof(model.CourseId), "Müəllimlər yalnız öz tədris etdikləri fənni seçərək elan paylaşa bilərlər.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCourseOptionsAsync(model);
                return View(model);
            }

            var payload = new
            {
                Title = model.Title,
                Content = model.Content,
                CourseId = model.CourseId > 0 ? model.CourseId : null
            };

            var (success, _, error) = await _apiService.PostWithResultAsync<object, AnnouncementItemViewModel>("api/announcements", payload);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Elan paylaşılan zaman xəta baş verdi.");
                await LoadCourseOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Elan uğurla paylaşıldı.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Announcements/Edit/{id}
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var announcement = await _apiService.GetAsync<AnnouncementItemViewModel>($"api/announcements/{id}");
            if (announcement == null)
            {
                TempData["ErrorMessage"] = "Elan tapılmadı.";
                return RedirectToAction(nameof(Index));
            }

            if (!announcement.CanEdit)
            {
                TempData["ErrorMessage"] = "Bu elanı redaktə etməyə icazəniz yoxdur.";
                return RedirectToAction(nameof(Index));
            }

            var model = new AnnouncementFormViewModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                CourseId = announcement.CourseId,
                IsAdmin = User.IsInRole("Admin"),
                IsTeacher = User.IsInRole("Teacher")
            };

            await LoadCourseOptionsAsync(model);
            return View(model);
        }

        // POST: /Announcements/Edit/{id}
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnnouncementFormViewModel model)
        {
            model.Id = id;
            model.IsAdmin = User.IsInRole("Admin");
            model.IsTeacher = User.IsInRole("Teacher");

            if (model.IsTeacher && !model.IsAdmin && (!model.CourseId.HasValue || model.CourseId.Value <= 0))
            {
                ModelState.AddModelError(nameof(model.CourseId), "Müəllimlər yalnız öz tədris etdikləri fənni seçə bilərlər.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCourseOptionsAsync(model);
                return View(model);
            }

            var payload = new
            {
                Title = model.Title,
                Content = model.Content,
                CourseId = model.CourseId > 0 ? model.CourseId : null
            };

            var (success, error) = await _apiService.PutWithResultAsync($"api/announcements/{id}", payload);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Elan yenilənərkən xəta baş verdi.");
                await LoadCourseOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Elan uğurla yeniləndi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Announcements/Delete/{id}
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _apiService.DeleteWithResultAsync($"api/announcements/{id}");
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Elan silinərkən xəta baş verdi.";
            }
            else
            {
                TempData["SuccessMessage"] = "Elan uğurla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCourseOptionsAsync(AnnouncementFormViewModel model)
        {
            var courses = await _apiService.GetAsync<List<AnnouncementCourseOptionViewModel>>("api/announcements/courses") 
                          ?? new List<AnnouncementCourseOptionViewModel>();

            var list = new List<SelectListItem>();

            if (model.IsAdmin)
            {
                list.Add(new SelectListItem
                {
                    Value = "",
                    Text = "📢 Müəssisə üzrə (Bütün universitet üçün ümumi elan)",
                    Selected = !model.CourseId.HasValue
                });
            }
            else
            {
                list.Add(new SelectListItem
                {
                    Value = "",
                    Text = "-- Fənni seçin --",
                    Disabled = true,
                    Selected = !model.CourseId.HasValue
                });
            }

            foreach (var c in courses)
            {
                list.Add(new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"📚 {c.Code} - {c.Name}",
                    Selected = model.CourseId == c.Id
                });
            }

            model.CourseOptions = list;
        }
    }
}
