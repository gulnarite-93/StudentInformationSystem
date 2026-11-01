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
    }
}
