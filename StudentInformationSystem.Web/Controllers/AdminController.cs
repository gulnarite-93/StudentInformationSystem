using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentInformationSystem.Web.Models;
using StudentInformationSystem.Web.Services;

namespace StudentInformationSystem.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IApiService _apiService;

        public AdminController(IApiService apiService)
        {
            _apiService = apiService;
        }

        #region 1. Şöbələr (Departments)

        [HttpGet]
        public async Task<IActionResult> Departments()
        {
            var list = await _apiService.GetAsync<List<DepartmentViewModel>>("api/departments") ?? new List<DepartmentViewModel>();
            return View(list);
        }

        [HttpGet]
        public IActionResult CreateDepartment()
        {
            return View(new DepartmentFormModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(DepartmentFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, _, error) = await _apiService.PostWithResultAsync<DepartmentFormModel, DepartmentViewModel>("api/departments", model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Şöbə əlavə edilərkən xəta baş verdi.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"'{model.Name}' şöbəsi uğurla əlavə edildi.";
            return RedirectToAction(nameof(Departments));
        }

        [HttpGet]
        public async Task<IActionResult> EditDepartment(int id)
        {
            var dept = await _apiService.GetAsync<DepartmentViewModel>($"api/departments/{id}");
            if (dept == null)
            {
                TempData["ErrorMessage"] = "Şöbə tapılmadı.";
                return RedirectToAction(nameof(Departments));
            }

            var formModel = new DepartmentFormModel
            {
                Id = dept.Id,
                Name = dept.Name,
                Code = dept.Code,
                Description = dept.Description
            };

            return View(formModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(int id, DepartmentFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, error) = await _apiService.PutWithResultAsync($"api/departments/{id}", model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Şöbə yenilənərkən xəta baş verdi.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"'{model.Name}' şöbəsi uğurla yeniləndi.";
            return RedirectToAction(nameof(Departments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var (success, error) = await _apiService.DeleteWithResultAsync($"api/departments/{id}");
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Şöbə silinə bilmədi.";
            }
            else
            {
                TempData["SuccessMessage"] = "Şöbə uğurla silindi.";
            }

            return RedirectToAction(nameof(Departments));
        }

        #endregion

        #region 2. Proqramlar (Programmes)

        [HttpGet]
        public async Task<IActionResult> Programmes()
        {
            var list = await _apiService.GetAsync<List<ProgrammeViewModel>>("api/programmes") ?? new List<ProgrammeViewModel>();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProgramme()
        {
            var model = new ProgrammeFormModel();
            await PopulateDepartmentsSelectList(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProgramme(ProgrammeFormModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsSelectList(model);
                return View(model);
            }

            var (success, _, error) = await _apiService.PostWithResultAsync<ProgrammeFormModel, ProgrammeViewModel>("api/programmes", model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Proqram əlavə edilərkən xəta baş verdi.");
                await PopulateDepartmentsSelectList(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"'{model.Name}' proqramı uğurla əlavə edildi.";
            return RedirectToAction(nameof(Programmes));
        }

        [HttpGet]
        public async Task<IActionResult> EditProgramme(int id)
        {
            var prog = await _apiService.GetAsync<ProgrammeViewModel>($"api/programmes/{id}");
            if (prog == null)
            {
                TempData["ErrorMessage"] = "Proqram tapılmadı.";
                return RedirectToAction(nameof(Programmes));
            }

            var formModel = new ProgrammeFormModel
            {
                Id = prog.Id,
                Name = prog.Name,
                Code = prog.Code,
                DepartmentId = prog.DepartmentId
            };

            await PopulateDepartmentsSelectList(formModel);
            return View(formModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProgramme(int id, ProgrammeFormModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsSelectList(model);
                return View(model);
            }

            var (success, error) = await _apiService.PutWithResultAsync($"api/programmes/{id}", model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Proqram yenilənərkən xəta baş verdi.");
                await PopulateDepartmentsSelectList(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"'{model.Name}' proqramı uğurla yeniləndi.";
            return RedirectToAction(nameof(Programmes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProgramme(int id)
        {
            var (success, error) = await _apiService.DeleteWithResultAsync($"api/programmes/{id}");
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Proqram silinə bilmədi.";
            }
            else
            {
                TempData["SuccessMessage"] = "Proqram uğurla silindi.";
            }

            return RedirectToAction(nameof(Programmes));
        }

        private async Task PopulateDepartmentsSelectList(ProgrammeFormModel model)
        {
            var depts = await _apiService.GetAsync<List<DepartmentViewModel>>("api/departments") ?? new List<DepartmentViewModel>();
            model.DepartmentList = depts.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = $"{d.Name} ({d.Code})"
            }).ToList();
        }

        #endregion

        #region 3. Akademik Dövrlər (Terms)

        [HttpGet]
        public async Task<IActionResult> Terms()
        {
            var list = await _apiService.GetAsync<List<TermViewModel>>("api/terms") ?? new List<TermViewModel>();
            return View(list);
        }

        [HttpGet]
        public IActionResult CreateTerm()
        {
            var model = new TermFormModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(4),
                IsActive = true
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTerm(TermFormModel model)
        {
            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "Bitmə tarixi başlanğıc tarixindən sonra olmalıdır.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, _, error) = await _apiService.PostWithResultAsync<TermFormModel, TermViewModel>("api/terms", model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Akademik dövr əlavə edilərkən xəta baş verdi.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"'{model.Name}' akademik dövrü uğurla əlavə edildi.";
            return RedirectToAction(nameof(Terms));
        }

        [HttpGet]
        public async Task<IActionResult> EditTerm(int id)
        {
            var term = await _apiService.GetAsync<TermViewModel>($"api/terms/{id}");
            if (term == null)
            {
                TempData["ErrorMessage"] = "Akademik dövr tapılmadı.";
                return RedirectToAction(nameof(Terms));
            }

            var formModel = new TermFormModel
            {
                Id = term.Id,
                Name = term.Name,
                StartDate = term.StartDate,
                EndDate = term.EndDate,
                IsActive = term.IsActive
            };

            return View(formModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTerm(int id, TermFormModel model)
        {
            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "Bitmə tarixi başlanğıc tarixindən sonra olmalıdır.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, error) = await _apiService.PutWithResultAsync($"api/terms/{id}", model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Akademik dövr yenilənərkən xəta baş verdi.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"'{model.Name}' akademik dövrü uğurla yeniləndi.";
            return RedirectToAction(nameof(Terms));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTerm(int id)
        {
            var (success, error) = await _apiService.DeleteWithResultAsync($"api/terms/{id}");
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Akademik dövr silinə bilmədi.";
            }
            else
            {
                TempData["SuccessMessage"] = "Akademik dövr uğurla silindi.";
            }

            return RedirectToAction(nameof(Terms));
        }

        #endregion
    }
}
