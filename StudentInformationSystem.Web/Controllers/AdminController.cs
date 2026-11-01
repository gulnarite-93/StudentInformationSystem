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

        #region 4. Tələbələr (F1: Students)

        [HttpGet]
        public async Task<IActionResult> Students(string? search = null, int? programmeId = null, int? academicYear = null)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search.Trim())}");
            if (programmeId.HasValue && programmeId.Value > 0) queryParams.Add($"programmeId={programmeId.Value}");
            if (academicYear.HasValue && academicYear.Value > 0) queryParams.Add($"academicYear={academicYear.Value}");

            var endpoint = "api/students" + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");
            var list = await _apiService.GetAsync<List<StudentViewModel>>(endpoint) ?? new List<StudentViewModel>();

            var programmes = await _apiService.GetAsync<List<ProgrammeViewModel>>("api/programmes") ?? new List<ProgrammeViewModel>();
            ViewBag.Programmes = programmes;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentProgrammeId = programmeId;
            ViewBag.CurrentAcademicYear = academicYear;

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> CreateStudent()
        {
            var model = new StudentFormModel();
            await PopulateStudentProgrammesSelectList(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(StudentFormModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Yeni tələbə üçün şifrə mütləq daxil edilməlidir.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateStudentProgrammesSelectList(model);
                return View(model);
            }

            var (success, _, error) = await _apiService.PostWithResultAsync<StudentFormModel, StudentViewModel>("api/students", model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Tələbə əlavə edilərkən xəta baş verdi.");
                await PopulateStudentProgrammesSelectList(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Tələbə '{model.FirstName} {model.LastName}' uğurla əlavə edildi.";
            return RedirectToAction(nameof(Students));
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await _apiService.GetAsync<StudentViewModel>($"api/students/{id}");
            if (student == null)
            {
                TempData["ErrorMessage"] = "Tələbə tapılmadı.";
                return RedirectToAction(nameof(Students));
            }

            var formModel = new StudentFormModel
            {
                Id = student.Id,
                Username = student.Username,
                Email = student.Email,
                FirstName = student.FirstName,
                LastName = student.LastName,
                StudentNumber = student.StudentNumber ?? string.Empty,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                AcademicYear = student.AcademicYear ?? 1,
                ProgrammeId = student.ProgrammeId ?? 0
            };

            await PopulateStudentProgrammesSelectList(formModel);
            return View(formModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(int id, StudentFormModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateStudentProgrammesSelectList(model);
                return View(model);
            }

            var updatePayload = new
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                StudentNumber = model.StudentNumber,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                AcademicYear = model.AcademicYear,
                ProgrammeId = model.ProgrammeId,
                NewPassword = model.Password
            };

            var (success, error) = await _apiService.PutWithResultAsync($"api/students/{id}", updatePayload);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Tələbə yenilənərkən xəta baş verdi.");
                await PopulateStudentProgrammesSelectList(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Tələbə '{model.FirstName} {model.LastName}' uğurla yeniləndi.";
            return RedirectToAction(nameof(Students));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var (success, error) = await _apiService.DeleteWithResultAsync($"api/students/{id}");
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Tələbə silinə bilmədi.";
            }
            else
            {
                TempData["SuccessMessage"] = "Tələbə uğurla silindi.";
            }

            return RedirectToAction(nameof(Students));
        }

        private async Task PopulateStudentProgrammesSelectList(StudentFormModel model)
        {
            var progs = await _apiService.GetAsync<List<ProgrammeViewModel>>("api/programmes") ?? new List<ProgrammeViewModel>();
            model.ProgrammeList = progs.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Name} ({p.DepartmentName})"
            }).ToList();
        }

        #endregion

        #region 5. Kurs Kataloqu və İdarəsi (F2: Courses)

        [HttpGet]
        public async Task<IActionResult> Courses(string? search = null, int? termId = null, int? programmeId = null, int? teacherId = null)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search.Trim())}");
            if (termId.HasValue && termId.Value > 0) queryParams.Add($"termId={termId.Value}");
            if (programmeId.HasValue && programmeId.Value > 0) queryParams.Add($"programmeId={programmeId.Value}");
            if (teacherId.HasValue && teacherId.Value > 0) queryParams.Add($"teacherId={teacherId.Value}");

            var endpoint = "api/courses" + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");
            var list = await _apiService.GetAsync<List<CourseViewModel>>(endpoint) ?? new List<CourseViewModel>();

            var terms = await _apiService.GetAsync<List<TermViewModel>>("api/terms") ?? new List<TermViewModel>();
            var programmes = await _apiService.GetAsync<List<ProgrammeViewModel>>("api/programmes") ?? new List<ProgrammeViewModel>();
            var teachers = await _apiService.GetAsync<List<TeacherSelectItem>>("api/courses/teachers") ?? new List<TeacherSelectItem>();

            ViewBag.Terms = terms;
            ViewBag.Programmes = programmes;
            ViewBag.Teachers = teachers;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentTermId = termId;
            ViewBag.CurrentProgrammeId = programmeId;
            ViewBag.CurrentTeacherId = teacherId;

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var model = new CourseFormModel();
            await PopulateCourseSelectLists(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CourseFormModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCourseSelectLists(model);
                return View(model);
            }

            var (success, _, error) = await _apiService.PostWithResultAsync<CourseFormModel, CourseViewModel>("api/courses", model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Kurs əlavə edilərkən xəta baş verdi.");
                await PopulateCourseSelectLists(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"'{model.Name}' kursu uğurla əlavə edildi.";
            return RedirectToAction(nameof(Courses));
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _apiService.GetAsync<CourseViewModel>($"api/courses/{id}");
            if (course == null)
            {
                TempData["ErrorMessage"] = "Kurs tapılmadı.";
                return RedirectToAction(nameof(Courses));
            }

            var formModel = new CourseFormModel
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Credits = course.Credits,
                Description = course.Description,
                ProgrammeId = course.ProgrammeId,
                TermId = course.TermId,
                TeacherId = course.TeacherId
            };

            await PopulateCourseSelectLists(formModel);
            return View(formModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, CourseFormModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCourseSelectLists(model);
                return View(model);
            }

            var (success, error) = await _apiService.PutWithResultAsync($"api/courses/{id}", model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Kurs yenilənərkən xəta baş verdi.");
                await PopulateCourseSelectLists(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"'{model.Name}' kursu uğurla yeniləndi.";
            return RedirectToAction(nameof(Courses));
        }

        [HttpGet]
        public async Task<IActionResult> AssignTeacher(int id)
        {
            var course = await _apiService.GetAsync<CourseViewModel>($"api/courses/{id}");
            if (course == null)
            {
                TempData["ErrorMessage"] = "Kurs tapılmadı.";
                return RedirectToAction(nameof(Courses));
            }

            var teachers = await _apiService.GetAsync<List<TeacherSelectItem>>("api/courses/teachers") ?? new List<TeacherSelectItem>();

            var model = new AssignTeacherViewModel
            {
                CourseId = course.Id,
                CourseCode = course.Code,
                CourseName = course.Name,
                CurrentTeacherName = course.TeacherName != null ? $"{course.TeacherTitle} {course.TeacherName}".Trim() : "Təyin olunmayıb",
                TeacherId = course.TeacherId,
                TeacherList = teachers.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"{t.Title} {t.FullName} ({t.DepartmentName ?? "Şöbə təyin edilməyib"})"
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTeacher(int id, AssignTeacherViewModel model)
        {
            var payload = new { TeacherId = model.TeacherId };
            var (success, error) = await _apiService.PutWithResultAsync($"api/courses/{id}/assign-teacher", payload);

            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Müəllim təyin edilərkən xəta baş verdi.";
                return RedirectToAction(nameof(AssignTeacher), new { id });
            }

            TempData["SuccessMessage"] = $"'{model.CourseName}' kursu üçün müəllim təyinatı uğurla yeniləndi.";
            return RedirectToAction(nameof(Courses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var (success, error) = await _apiService.DeleteWithResultAsync($"api/courses/{id}");
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Kurs silinə bilmədi.";
            }
            else
            {
                TempData["SuccessMessage"] = "Kurs uğurla silindi.";
            }

            return RedirectToAction(nameof(Courses));
        }

        private async Task PopulateCourseSelectLists(CourseFormModel model)
        {
            var progs = await _apiService.GetAsync<List<ProgrammeViewModel>>("api/programmes") ?? new List<ProgrammeViewModel>();
            var terms = await _apiService.GetAsync<List<TermViewModel>>("api/terms") ?? new List<TermViewModel>();
            var teachers = await _apiService.GetAsync<List<TeacherSelectItem>>("api/courses/teachers") ?? new List<TeacherSelectItem>();

            model.ProgrammeList = progs.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Name} ({p.DepartmentName})"
            }).ToList();

            model.TermList = terms.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.IsActive ? $"{t.Name} (Aktiv)" : t.Name
            }).ToList();

            model.TeacherList = teachers.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.Title} {t.FullName} ({t.DepartmentName ?? "Şöbə təyin edilməyib"})"
            }).ToList();
        }

        #endregion

        #region F3: Kurs Qeydiyyatı İdarəsi (Admin)

        [HttpGet]
        public async Task<IActionResult> StudentEnrollments(int studentId)
        {
            var student = await _apiService.GetAsync<StudentViewModel>($"api/students/{studentId}");
            if (student == null)
            {
                TempData["ErrorMessage"] = "Tələbə tapılmadı.";
                return RedirectToAction(nameof(Students));
            }

            var enrollments = await _apiService.GetAsync<List<EnrollmentViewModel>>($"api/enrollments/student/{studentId}")
                ?? new List<EnrollmentViewModel>();

            var allCourses = await _apiService.GetAsync<List<CourseViewModel>>("api/courses")
                ?? new List<CourseViewModel>();

            var model = new AdminStudentEnrollmentsViewModel
            {
                Student = student,
                Enrollments = enrollments,
                AvailableCourses = allCourses
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminEnrollStudent(int studentId, int courseId)
        {
            var (success, _, error) = await _apiService.PostWithResultAsync<object, EnrollmentViewModel>(
                "api/enrollments/enroll",
                new { StudentId = studentId, CourseId = courseId });

            if (success)
            {
                TempData["SuccessMessage"] = "Tələbə kursa uğurla əlavə edildi.";
            }
            else
            {
                TempData["ErrorMessage"] = error ?? "Tələbə kursa əlavə edilərkən xəta baş verdi.";
            }

            return RedirectToAction(nameof(StudentEnrollments), new { studentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminDropStudent(int enrollmentId, int studentId)
        {
            var (success, _, error) = await _apiService.PostWithResultAsync<object, object>(
                $"api/enrollments/{enrollmentId}/drop",
                new { });

            if (success)
            {
                TempData["SuccessMessage"] = "Tələbənin kurs qeydiyyatı ləğv edildi (çıxarıldı).";
            }
            else
            {
                TempData["ErrorMessage"] = error ?? "Qeydiyyat ləğv edilərkən xəta baş verdi.";
            }

            return RedirectToAction(nameof(StudentEnrollments), new { studentId });
        }

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

        #endregion
    }
}
