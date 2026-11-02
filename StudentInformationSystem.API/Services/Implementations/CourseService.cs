using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.DTOs.Courses;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Repositories;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly IRepository<Course> _courseRepo;
        private readonly IRepository<Programme> _programmeRepo;
        private readonly IRepository<Term> _termRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Enrollment> _enrollmentRepo;
        private readonly IRepository<Attendance> _attendanceRepo;

        public CourseService(
            IRepository<Course> courseRepo,
            IRepository<Programme> programmeRepo,
            IRepository<Term> termRepo,
            IRepository<User> userRepo,
            IRepository<Enrollment> enrollmentRepo,
            IRepository<Attendance> attendanceRepo)
        {
            _courseRepo = courseRepo;
            _programmeRepo = programmeRepo;
            _termRepo = termRepo;
            _userRepo = userRepo;
            _enrollmentRepo = enrollmentRepo;
            _attendanceRepo = attendanceRepo;
        }

        public async Task<IReadOnlyList<CourseDto>> GetAllAsync(string? search = null, int? termId = null, int? programmeId = null, int? teacherId = null)
        {
            var query = _courseRepo.Query()
                .Include(c => c.Programme)
                    .ThenInclude(p => p.Department)
                .Include(c => c.Term)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    c.Code.ToLower().Contains(term) ||
                    (c.Description != null && c.Description.ToLower().Contains(term)));
            }

            if (termId.HasValue && termId.Value > 0)
            {
                query = query.Where(c => c.TermId == termId.Value);
            }

            if (programmeId.HasValue && programmeId.Value > 0)
            {
                query = query.Where(c => c.ProgrammeId == programmeId.Value);
            }

            if (teacherId.HasValue && teacherId.Value > 0)
            {
                query = query.Where(c => c.TeacherId == teacherId.Value);
            }

            return await query
                .OrderBy(c => c.Code)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Credits = c.Credits,
                    Description = c.Description,
                    ProgrammeId = c.ProgrammeId,
                    ProgrammeName = c.Programme.Name,
                    DepartmentName = c.Programme.Department != null ? c.Programme.Department.Name : string.Empty,
                    TermId = c.TermId,
                    TermName = c.Term.Name,
                    IsTermActive = c.Term.IsActive,
                    TeacherId = c.TeacherId,
                    TeacherName = c.Teacher != null ? $"{c.Teacher.FirstName} {c.Teacher.LastName}".Trim() : null,
                    TeacherTitle = c.Teacher != null ? c.Teacher.Title : null,
                    EnrolledStudentsCount = c.Enrollments.Count(e => e.Status == "Enrolled")
                })
                .ToListAsync();
        }

        public async Task<CourseDto?> GetByIdAsync(int id)
        {
            return await _courseRepo.Query()
                .Include(c => c.Programme)
                    .ThenInclude(p => p.Department)
                .Include(c => c.Term)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Where(c => c.Id == id)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Credits = c.Credits,
                    Description = c.Description,
                    ProgrammeId = c.ProgrammeId,
                    ProgrammeName = c.Programme.Name,
                    DepartmentName = c.Programme.Department != null ? c.Programme.Department.Name : string.Empty,
                    TermId = c.TermId,
                    TermName = c.Term.Name,
                    IsTermActive = c.Term.IsActive,
                    TeacherId = c.TeacherId,
                    TeacherName = c.Teacher != null ? $"{c.Teacher.FirstName} {c.Teacher.LastName}".Trim() : null,
                    TeacherTitle = c.Teacher != null ? c.Teacher.Title : null,
                    EnrolledStudentsCount = c.Enrollments.Count(e => e.Status == "Enrolled")
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<TeacherSimpleDto>> GetTeachersListAsync()
        {
            return await _userRepo.Query()
                .Include(u => u.Department)
                .Where(u => u.Role == UserRoles.Teacher)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new TeacherSimpleDto
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}".Trim(),
                    Title = u.Title,
                    DepartmentName = u.Department != null ? u.Department.Name : null
                })
                .ToListAsync();
        }

        public async Task<(bool Success, CourseDto? Course, string? ErrorMessage)> CreateAsync(CourseCreateDto dto)
        {
            var codeExists = await _courseRepo.Query().AnyAsync(c => c.Code.ToLower() == dto.Code.Trim().ToLower() && c.TermId == dto.TermId);
            if (codeExists)
            {
                return (false, null, "Bu semestr üçün eyni kodlu kurs artıq mövcuddur.");
            }

            var programme = await _programmeRepo.Query().Include(p => p.Department).FirstOrDefaultAsync(p => p.Id == dto.ProgrammeId);
            if (programme == null)
            {
                return (false, null, "Seçilmiş proqram tapılmadı.");
            }

            var term = await _termRepo.GetByIdAsync(dto.TermId);
            if (term == null)
            {
                return (false, null, "Seçilmiş akademik dövr tapılmadı.");
            }

            User? teacher = null;
            if (dto.TeacherId.HasValue && dto.TeacherId.Value > 0)
            {
                teacher = await _userRepo.Query().FirstOrDefaultAsync(u => u.Id == dto.TeacherId.Value && u.Role == UserRoles.Teacher);
                if (teacher == null)
                {
                    return (false, null, "Seçilmiş müəllim tapılmadı.");
                }
            }

            var course = new Course
            {
                Code = dto.Code.Trim().ToUpper(),
                Name = dto.Name.Trim(),
                Credits = dto.Credits,
                Description = dto.Description?.Trim(),
                ProgrammeId = dto.ProgrammeId,
                TermId = dto.TermId,
                TeacherId = dto.TeacherId > 0 ? dto.TeacherId : null
            };

            await _courseRepo.AddAsync(course);

            var courseDto = new CourseDto
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Credits = course.Credits,
                Description = course.Description,
                ProgrammeId = course.ProgrammeId,
                ProgrammeName = programme.Name,
                DepartmentName = programme.Department != null ? programme.Department.Name : string.Empty,
                TermId = course.TermId,
                TermName = term.Name,
                IsTermActive = term.IsActive,
                TeacherId = course.TeacherId,
                TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}".Trim() : null,
                TeacherTitle = teacher != null ? teacher.Title : null,
                EnrolledStudentsCount = 0
            };

            return (true, courseDto, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, CourseUpdateDto dto)
        {
            var course = await _courseRepo.GetByIdAsync(id);
            if (course == null)
            {
                return (false, "Kurs tapılmadı.");
            }

            var codeExists = await _courseRepo.Query().AnyAsync(c => c.Id != id && c.Code.ToLower() == dto.Code.Trim().ToLower() && c.TermId == dto.TermId);
            if (codeExists)
            {
                return (false, "Bu semestr üçün eyni kodlu kurs artıq mövcuddur.");
            }

            var programme = await _programmeRepo.GetByIdAsync(dto.ProgrammeId);
            if (programme == null)
            {
                return (false, "Seçilmiş proqram tapılmadı.");
            }

            var term = await _termRepo.GetByIdAsync(dto.TermId);
            if (term == null)
            {
                return (false, "Seçilmiş akademik dövr tapılmadı.");
            }

            if (dto.TeacherId.HasValue && dto.TeacherId.Value > 0)
            {
                var teacherExists = await _userRepo.Query().AnyAsync(u => u.Id == dto.TeacherId.Value && u.Role == UserRoles.Teacher);
                if (!teacherExists)
                {
                    return (false, "Seçilmiş müəllim tapılmadı.");
                }
            }

            course.Code = dto.Code.Trim().ToUpper();
            course.Name = dto.Name.Trim();
            course.Credits = dto.Credits;
            course.Description = dto.Description?.Trim();
            course.ProgrammeId = dto.ProgrammeId;
            course.TermId = dto.TermId;
            course.TeacherId = dto.TeacherId > 0 ? dto.TeacherId : null;

            await _courseRepo.UpdateAsync(course);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> AssignTeacherAsync(int courseId, int? teacherId)
        {
            var course = await _courseRepo.GetByIdAsync(courseId);
            if (course == null)
            {
                return (false, "Kurs tapılmadı.");
            }

            if (teacherId.HasValue && teacherId.Value > 0)
            {
                var teacher = await _userRepo.Query().FirstOrDefaultAsync(u => u.Id == teacherId.Value && u.Role == UserRoles.Teacher);
                if (teacher == null)
                {
                    return (false, "Seçilmiş müəllim tapılmadı.");
                }
                course.TeacherId = teacherId.Value;
            }
            else
            {
                course.TeacherId = null;
            }

            await _courseRepo.UpdateAsync(course);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            var course = await _courseRepo.GetByIdAsync(id);
            if (course == null)
            {
                return (false, "Kurs tapılmadı.");
            }

            var hasEnrollments = await _enrollmentRepo.Query().AnyAsync(e => e.CourseId == id);
            if (hasEnrollments)
            {
                return (false, "Bu kursa qeydiyyatdan keçmiş tələbələr olduğu üçün silinə bilməz.");
            }

            var hasAttendances = await _attendanceRepo.Query().AnyAsync(a => a.CourseId == id);
            if (hasAttendances)
            {
                return (false, "Bu kurs üzrə dərs davamiyyəti qeyd edildiyi üçün silinə bilməz.");
            }

            await _courseRepo.DeleteAsync(course);
            return (true, null);
        }
    }
}
