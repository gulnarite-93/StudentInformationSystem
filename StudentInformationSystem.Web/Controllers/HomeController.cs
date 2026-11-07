using Microsoft.AspNetCore.Mvc;
using StudentInformationSystem.Web.Models;
using System.Diagnostics;

namespace StudentInformationSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Student"))
                {
                    return RedirectToAction("Dashboard", "Student");
                }
                if (User.IsInRole("Teacher"))
                {
                    return RedirectToAction("Grades", "Teacher");
                }
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Departments", "Admin");
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
