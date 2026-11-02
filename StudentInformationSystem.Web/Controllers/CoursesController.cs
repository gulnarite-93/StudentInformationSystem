using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.Web.Models;
using StudentInformationSystem.Web.Services;

namespace StudentInformationSystem.Web.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly IApiService _apiService;

        public CoursesController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search = null, int? termId = null, int? programmeId = null)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search.Trim())}");
            if (termId.HasValue && termId.Value > 0) queryParams.Add($"termId={termId.Value}");
            if (programmeId.HasValue && programmeId.Value > 0) queryParams.Add($"programmeId={programmeId.Value}");

            var endpoint = "api/courses" + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");
            var list = await _apiService.GetAsync<List<CourseViewModel>>(endpoint) ?? new List<CourseViewModel>();

            var terms = await _apiService.GetAsync<List<TermViewModel>>("api/terms") ?? new List<TermViewModel>();
            var programmes = await _apiService.GetAsync<List<ProgrammeViewModel>>("api/programmes") ?? new List<ProgrammeViewModel>();

            ViewBag.Terms = terms;
            ViewBag.Programmes = programmes;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentTermId = termId;
            ViewBag.CurrentProgrammeId = programmeId;

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _apiService.GetAsync<CourseViewModel>($"api/courses/{id}");
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
    }
}
