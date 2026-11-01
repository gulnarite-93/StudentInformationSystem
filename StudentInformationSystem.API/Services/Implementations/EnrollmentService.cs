using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.DTOs.Enrollments;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Repositories;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Services.Implementations
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IRepository<Enrollment> _enrollmentRepo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Term> _termRepo;

        public EnrollmentService(
            IRepository<Enrollment> enrollmentRepo,
            IRepository<Course> courseRepo,
            IRepository<User> userRepo,
            IRepository<Term> termRepo)
        {
            _enrollmentRepo = enrollmentRepo;
            _courseRepo = courseRepo;
            _userRepo = userRepo;
            _termRepo = termRepo;
        }

        public async Task<(bool Success, EnrollmentDto? Enrollment, string? ErrorMessage)> EnrollAsync(int studentId, int courseId, bool isAdmin = false)
        {
            var student = await _userRepo.Query()
                .Include(u => u.Programme)
                .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == UserRoles.Student);

            if (student == null)
            {
                return (false, null, "Tələbə tapılmadı və ya istifadəçi tələbə rolunda deyil.");
            }

            var course = await _courseRepo.Query()
                .Include(c => c.Term)
                .Include(c => c.Programme)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return (false, null, "Kurs tapılmadı.");
            }

            // Qayda: Yalnız aktiv akademik dövrün fənlərinə yazılmaq olar (Admin istisna ola bilər)
            if (!isAdmin && !course.Term.IsActive)
            {
                return (false, null, "Yalnız aktiv akademik dövrün (semestrin) kurslarına qeydiyyatdan keçmək olar.");
            }

            // Qayda: Eyni kursa iki dəfə yazılmaq olmaz
            var existing = await _enrollmentRepo.Query()
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (existing != null)
            {
                if (existing.Status == "Enrolled")
                {
                    return (false, null, "Siz artıq bu kursa qeydiyyatdan keçmisiniz (eyni kursa təkrar yazılmaq olmaz).");
                }

                // Əgər əvvəl çıxmışdısa (Dropped), statusu yenidən "Enrolled" edirik
                existing.Status = "Enrolled";
                existing.EnrollmentDate = DateTime.UtcNow;
                await _enrollmentRepo.UpdateAsync(existing);

                return (true, MapToDto(existing, student, course), null);
            }

            var newEnrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.UtcNow,
                Status = "Enrolled"
            };

            await _enrollmentRepo.AddAsync(newEnrollment);

            return (true, MapToDto(newEnrollment, student, course), null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DropAsync(int enrollmentId, int currentUserId, string currentUserRole)
        {
            var enrollment = await _enrollmentRepo.Query()
                .Include(e => e.Course).ThenInclude(c => c.Term)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
            {
                return (false, "Qeydiyyat tapılmadı.");
            }

            // Qayda: Tələbə yalnız öz adından imtina edə bilər
            if (currentUserRole != UserRoles.Admin && enrollment.StudentId != currentUserId)
            {
                return (false, "Yalnız öz qeydiyyatınızdan imtina edə bilərsiniz.");
            }

            // Qayda: Yalnız aktiv akademik dövrün kursundan imtina edilə bilər
            if (currentUserRole != UserRoles.Admin && !enrollment.Course.Term.IsActive)
            {
                return (false, "Aktiv olmayan akademik dövrün kursundan imtina etmək olmaz.");
            }

            enrollment.Status = "Dropped";
            await _enrollmentRepo.UpdateAsync(enrollment);

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DropByCourseAsync(int studentId, int courseId, int currentUserId, string currentUserRole)
        {
            if (currentUserRole != UserRoles.Admin && studentId != currentUserId)
            {
                return (false, "Yalnız öz qeydiyyatınızdan imtina edə bilərsiniz.");
            }

            var enrollment = await _enrollmentRepo.Query()
                .Include(e => e.Course).ThenInclude(c => c.Term)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId && e.Status == "Enrolled");

            if (enrollment == null)
            {
                return (false, "Bu kurs üzrə aktiv qeydiyyat tapılmadı.");
            }

            if (currentUserRole != UserRoles.Admin && !enrollment.Course.Term.IsActive)
            {
                return (false, "Aktiv olmayan akademik dövrün kursundan imtina etmək olmaz.");
            }

            enrollment.Status = "Dropped";
            await _enrollmentRepo.UpdateAsync(enrollment);

            return (true, null);
        }

        public async Task<IReadOnlyList<EnrollmentDto>> GetStudentEnrollmentsAsync(int studentId, int? termId = null, string? status = null)
        {
            var query = _enrollmentRepo.Query()
                .Include(e => e.Student).ThenInclude(s => s.Programme)
                .Include(e => e.Course).ThenInclude(c => c.Term)
                .Include(e => e.Course).ThenInclude(c => c.Programme)
                .Include(e => e.Course).ThenInclude(c => c.Teacher)
                .Where(e => e.StudentId == studentId);

            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                query = query.Where(e => e.Status == status);
            }
            else if (string.IsNullOrEmpty(status))
            {
                // Standart olaraq yalnız aktiv qeydiyyatlar
                query = query.Where(e => e.Status == "Enrolled");
            }

            if (termId.HasValue && termId.Value > 0)
            {
                query = query.Where(e => e.Course.TermId == termId.Value);
            }

            return await query
                .OrderByDescending(e => e.Course.Term.IsActive)
                .ThenBy(e => e.Course.Code)
                .Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = $"{e.Student.FirstName} {e.Student.LastName}".Trim(),
                    StudentEmail = e.Student.Email,
                    StudentNumber = e.Student.StudentNumber,
                    ProgrammeName = e.Student.Programme != null ? e.Student.Programme.Name : string.Empty,
                    CourseId = e.CourseId,
                    CourseCode = e.Course.Code,
                    CourseName = e.Course.Name,
                    Credits = e.Course.Credits,
                    TeacherId = e.Course.TeacherId,
                    TeacherName = e.Course.Teacher != null ? $"{e.Course.Teacher.FirstName} {e.Course.Teacher.LastName}".Trim() : null,
                    TeacherTitle = e.Course.Teacher != null ? e.Course.Teacher.Title : null,
                    TermId = e.Course.TermId,
                    TermName = e.Course.Term.Name,
                    IsTermActive = e.Course.Term.IsActive,
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<EnrolledStudentDto>> GetCourseEnrolledStudentsAsync(int courseId, int currentUserId, string currentUserRole)
        {
            var course = await _courseRepo.GetByIdAsync(courseId);
            if (course == null)
            {
                return Array.Empty<EnrolledStudentDto>();
            }

            // Əgər müəllimdirsə, yalnız öz kursunun tələbələrini görə bilər
            if (currentUserRole == UserRoles.Teacher && course.TeacherId != currentUserId)
            {
                return Array.Empty<EnrolledStudentDto>();
            }

            return await _enrollmentRepo.Query()
                .Include(e => e.Student).ThenInclude(s => s.Programme)
                .Where(e => e.CourseId == courseId && e.Status == "Enrolled")
                .OrderBy(e => e.Student.LastName)
                .ThenBy(e => e.Student.FirstName)
                .Select(e => new EnrolledStudentDto
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    FullName = $"{e.Student.FirstName} {e.Student.LastName}".Trim(),
                    Email = e.Student.Email,
                    StudentNumber = e.Student.StudentNumber,
                    PhoneNumber = e.Student.PhoneNumber,
                    ProgrammeName = e.Student.Programme != null ? e.Student.Programme.Name : string.Empty,
                    AcademicYear = e.Student.AcademicYear,
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<StudentCourseRegistrationDto>> GetAvailableCoursesForStudentAsync(int studentId)
        {
            var student = await _userRepo.Query()
                .Include(u => u.Programme)
                .FirstOrDefaultAsync(u => u.Id == studentId);

            if (student == null)
            {
                return Array.Empty<StudentCourseRegistrationDto>();
            }

            // Cari aktiv semestrin kursları
            var courses = await _courseRepo.Query()
                .Include(c => c.Programme).ThenInclude(p => p.Department)
                .Include(c => c.Term)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Where(c => c.Term.IsActive)
                .OrderBy(c => c.Code)
                .ToListAsync();

            var studentEnrollments = await _enrollmentRepo.Query()
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

            return courses.Select(c =>
            {
                var enr = studentEnrollments.FirstOrDefault(e => e.CourseId == c.Id && e.Status == "Enrolled");
                return new StudentCourseRegistrationDto
                {
                    CourseId = c.Id,
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
                    EnrolledStudentsCount = c.Enrollments.Count(e => e.Status == "Enrolled"),
                    IsAlreadyEnrolled = enr != null,
                    EnrollmentId = enr?.Id,
                    EnrollmentStatus = enr?.Status
                };
            }).ToList();
        }

        private static EnrollmentDto MapToDto(Enrollment e, User student, Course course)
        {
            return new EnrollmentDto
            {
                Id = e.Id,
                StudentId = student.Id,
                StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                StudentEmail = student.Email,
                StudentNumber = student.StudentNumber,
                ProgrammeName = student.Programme != null ? student.Programme.Name : string.Empty,
                CourseId = course.Id,
                CourseCode = course.Code,
                CourseName = course.Name,
                Credits = course.Credits,
                TeacherId = course.TeacherId,
                TeacherName = course.Teacher != null ? $"{course.Teacher.FirstName} {course.Teacher.LastName}".Trim() : null,
                TeacherTitle = course.Teacher != null ? course.Teacher.Title : null,
                TermId = course.TermId,
                TermName = course.Term.Name,
                IsTermActive = course.Term.IsActive,
                EnrollmentDate = e.EnrollmentDate,
                Status = e.Status
            };
        }
    }
}
