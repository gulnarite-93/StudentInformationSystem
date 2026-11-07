using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.DTOs.Attendances;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Repositories;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IRepository<Attendance> _attendanceRepo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IRepository<Enrollment> _enrollmentRepo;
        private readonly IRepository<User> _userRepo;

        public AttendanceService(
            IRepository<Attendance> attendanceRepo,
            IRepository<Course> courseRepo,
            IRepository<Enrollment> enrollmentRepo,
            IRepository<User> userRepo)
        {
            _attendanceRepo = attendanceRepo;
            _courseRepo = courseRepo;
            _enrollmentRepo = enrollmentRepo;
            _userRepo = userRepo;
        }

        public async Task<IReadOnlyList<LessonSessionSummaryDto>> GetCourseSessionsAsync(int courseId, int currentUserId, string currentUserRole)
        {
            var course = await _courseRepo.Query()
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return Array.Empty<LessonSessionSummaryDto>();
            }

            var (allowed, _) = VerifyCourseAccess(course, currentUserId, currentUserRole);
            if (!allowed)
            {
                return Array.Empty<LessonSessionSummaryDto>();
            }

            var attendances = await _attendanceRepo.Query()
                .Where(a => a.CourseId == courseId)
                .ToListAsync();

            var sessions = attendances
                .GroupBy(a => a.LessonDate.Date)
                .Select(g => new LessonSessionSummaryDto
                {
                    CourseId = course.Id,
                    CourseCode = course.Code,
                    CourseName = course.Name,
                    LessonDate = g.Key,
                    LessonTopic = g.FirstOrDefault(a => !string.IsNullOrEmpty(a.LessonTopic))?.LessonTopic ?? "Mövzu qeyd edilməyib",
                    TotalStudents = g.Count(),
                    PresentCount = g.Count(a => a.Status == AttendanceStatus.Present || (string.IsNullOrEmpty(a.Status) && a.IsPresent)),
                    AbsentCount = g.Count(a => a.Status == AttendanceStatus.Absent || (string.IsNullOrEmpty(a.Status) && !a.IsPresent && a.Note != "Excused")),
                    ExcusedCount = g.Count(a => a.Status == AttendanceStatus.Excused || a.Note == "Excused" || a.Note == "İcazəli")
                })
                .OrderByDescending(s => s.LessonDate)
                .ToList();

            return sessions;
        }

        public async Task<LessonSessionDetailsDto?> GetSessionDetailsAsync(int courseId, DateTime lessonDate, int currentUserId, string currentUserRole)
        {
            var course = await _courseRepo.Query()
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return null;

            var (allowed, _) = VerifyCourseAccess(course, currentUserId, currentUserRole);
            if (!allowed) return null;

            var targetDate = lessonDate.Date;
            var nextDate = targetDate.AddDays(1);

            var attendances = await _attendanceRepo.Query()
                .Include(a => a.Student)
                .Where(a => a.CourseId == courseId && a.LessonDate >= targetDate && a.LessonDate < nextDate)
                .OrderBy(a => a.Student.LastName)
                .ThenBy(a => a.Student.FirstName)
                .ToListAsync();

            if (!attendances.Any()) return null;

            var topic = attendances.FirstOrDefault(a => !string.IsNullOrEmpty(a.LessonTopic))?.LessonTopic ?? string.Empty;

            return new LessonSessionDetailsDto
            {
                CourseId = course.Id,
                CourseCode = course.Code,
                CourseName = course.Name,
                LessonDate = targetDate,
                LessonTopic = topic,
                Records = attendances.Select(a => new AttendanceRecordDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = $"{a.Student.FirstName} {a.Student.LastName}".Trim(),
                    StudentNumber = a.Student.StudentNumber,
                    Status = string.IsNullOrEmpty(a.Status) 
                        ? (a.IsPresent ? AttendanceStatus.Present : AttendanceStatus.Absent)
                        : a.Status,
                    Note = a.Note
                }).ToList()
            };
        }

        public async Task<LessonSessionDetailsDto> GetNewSessionTemplateAsync(int courseId, int currentUserId, string currentUserRole)
        {
            var course = await _courseRepo.Query()
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                throw new ArgumentException("Kurs tapılmadı.");
            }

            var (allowed, error) = VerifyCourseAccess(course, currentUserId, currentUserRole);
            if (!allowed)
            {
                throw new UnauthorizedAccessException(error);
            }

            // Kursa aktiv yazılmış bütün tələbələr
            var enrolledStudents = await _enrollmentRepo.Query()
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId && e.Status == "Enrolled")
                .OrderBy(e => e.Student.LastName)
                .ThenBy(e => e.Student.FirstName)
                .Select(e => e.Student)
                .ToListAsync();

            return new LessonSessionDetailsDto
            {
                CourseId = course.Id,
                CourseCode = course.Code,
                CourseName = course.Name,
                LessonDate = DateTime.Today,
                LessonTopic = string.Empty,
                Records = enrolledStudents.Select(s => new AttendanceRecordDto
                {
                    Id = 0,
                    StudentId = s.Id,
                    StudentName = $"{s.FirstName} {s.LastName}".Trim(),
                    StudentNumber = s.StudentNumber,
                    Status = AttendanceStatus.Present,
                    Note = string.Empty
                }).ToList()
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateSessionAsync(CreateLessonSessionDto dto, int currentUserId, string currentUserRole)
        {
            var course = await _courseRepo.Query()
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == dto.CourseId);

            if (course == null)
            {
                return (false, "Kurs tapılmadı.");
            }

            var (allowed, error) = VerifyCourseAccess(course, currentUserId, currentUserRole);
            if (!allowed)
            {
                return (false, error);
            }

            var targetDate = dto.LessonDate.Date;
            var nextDate = targetDate.AddDays(1);

            // Qayda: Eyni kurs üçün eyni tarixdə iki dəfə sessiya yaradıla bilməz
            var existingSession = await _attendanceRepo.Query()
                .AnyAsync(a => a.CourseId == dto.CourseId && a.LessonDate >= targetDate && a.LessonDate < nextDate);

            if (existingSession)
            {
                return (false, $"Bu kurs üçün {targetDate:dd.MM.yyyy} tarixində artıq dərs sessiyası mövcuddur. Zəhmət olmasa siyahıdan həmin sessiyanı redaktə edin.");
            }

            if (!dto.Records.Any())
            {
                return (false, "Davamiyyət üçün heç bir tələbə qeydi göndərilməyib.");
            }

            foreach (var item in dto.Records)
            {
                var status = string.IsNullOrWhiteSpace(item.Status) ? AttendanceStatus.Present : item.Status;
                var isPresent = (status == AttendanceStatus.Present);

                var attendance = new Attendance
                {
                    CourseId = dto.CourseId,
                    StudentId = item.StudentId,
                    LessonDate = targetDate,
                    LessonTopic = dto.LessonTopic.Trim(),
                    Status = status,
                    IsPresent = isPresent,
                    Note = item.Note?.Trim()
                };

                await _attendanceRepo.AddAsync(attendance);
            }

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateSessionAsync(UpdateLessonSessionDto dto, int currentUserId, string currentUserRole)
        {
            var course = await _courseRepo.Query()
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == dto.CourseId);

            if (course == null)
            {
                return (false, "Kurs tapılmadı.");
            }

            var (allowed, error) = VerifyCourseAccess(course, currentUserId, currentUserRole);
            if (!allowed)
            {
                return (false, error);
            }

            var originalDate = dto.OriginalLessonDate.Date;
            var origNext = originalDate.AddDays(1);
            var newDate = dto.LessonDate.Date;
            var newNext = newDate.AddDays(1);

            // Əgər tarixi dəyişibsə və yeni tarixdə artıq başqa bir sessiya varsa, xəbərdarlıq edirik
            if (originalDate != newDate)
            {
                var dateOccupied = await _attendanceRepo.Query()
                    .AnyAsync(a => a.CourseId == dto.CourseId && a.LessonDate >= newDate && a.LessonDate < newNext);

                if (dateOccupied)
                {
                    return (false, $"{newDate:dd.MM.yyyy} tarixində bu kurs üçün artıq başqa bir dərs sessiyası mövcuddur.");
                }
            }

            var existingAttendances = await _attendanceRepo.Query()
                .Where(a => a.CourseId == dto.CourseId && a.LessonDate >= originalDate && a.LessonDate < origNext)
                .ToListAsync();

            if (!existingAttendances.Any())
            {
                return (false, "Yenilənmək üçün sessiya tapılmadı.");
            }

            foreach (var item in dto.Records)
            {
                var status = string.IsNullOrWhiteSpace(item.Status) ? AttendanceStatus.Present : item.Status;
                var isPresent = (status == AttendanceStatus.Present);
                var existing = existingAttendances.FirstOrDefault(a => a.StudentId == item.StudentId);

                if (existing != null)
                {
                    existing.LessonDate = newDate;
                    existing.LessonTopic = dto.LessonTopic.Trim();
                    existing.Status = status;
                    existing.IsPresent = isPresent;
                    existing.Note = item.Note?.Trim();

                    await _attendanceRepo.UpdateAsync(existing);
                }
                else
                {
                    var newAttendance = new Attendance
                    {
                        CourseId = dto.CourseId,
                        StudentId = item.StudentId,
                        LessonDate = newDate,
                        LessonTopic = dto.LessonTopic.Trim(),
                        Status = status,
                        IsPresent = isPresent,
                        Note = item.Note?.Trim()
                    };

                    await _attendanceRepo.AddAsync(newAttendance);
                }
            }

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteSessionAsync(int courseId, DateTime lessonDate, int currentUserId, string currentUserRole)
        {
            var course = await _courseRepo.Query()
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return (false, "Kurs tapılmadı.");

            var (allowed, error) = VerifyCourseAccess(course, currentUserId, currentUserRole);
            if (!allowed) return (false, error);

            var targetDate = lessonDate.Date;
            var nextDate = targetDate.AddDays(1);

            var attendances = await _attendanceRepo.Query()
                .Where(a => a.CourseId == courseId && a.LessonDate >= targetDate && a.LessonDate < nextDate)
                .ToListAsync();

            if (!attendances.Any())
            {
                return (false, "Silinəcək dərs sessiyası tapılmadı.");
            }

            foreach (var att in attendances)
            {
                await _attendanceRepo.DeleteAsync(att);
            }

            return (true, null);
        }

        public async Task<IReadOnlyList<StudentCourseAttendanceSummaryDto>> GetStudentAttendanceAsync(int studentId)
        {
            var enrollments = await _enrollmentRepo.Query()
                .Include(e => e.Course).ThenInclude(c => c.Term)
                .Include(e => e.Course).ThenInclude(c => c.Teacher)
                .Where(e => e.StudentId == studentId && e.Status == "Enrolled")
                .ToListAsync();

            var allAttendances = await _attendanceRepo.Query()
                .Where(a => a.StudentId == studentId)
                .ToListAsync();

            var result = new List<StudentCourseAttendanceSummaryDto>();

            foreach (var enr in enrollments)
            {
                var courseAttendances = allAttendances
                    .Where(a => a.CourseId == enr.CourseId)
                    .OrderByDescending(a => a.LessonDate)
                    .ToList();

                var total = courseAttendances.Count;
                var present = courseAttendances.Count(a => a.Status == AttendanceStatus.Present || (string.IsNullOrEmpty(a.Status) && a.IsPresent));
                var absent = courseAttendances.Count(a => a.Status == AttendanceStatus.Absent || (string.IsNullOrEmpty(a.Status) && !a.IsPresent && a.Note != "Excused"));
                var excused = courseAttendances.Count(a => a.Status == AttendanceStatus.Excused || a.Note == "Excused" || a.Note == "İcazəli");

                double percentage = 100.0;
                if (total > 0)
                {
                    percentage = Math.Round(((double)present / total) * 100, 1);
                }

                result.Add(new StudentCourseAttendanceSummaryDto
                {
                    CourseId = enr.CourseId,
                    CourseCode = enr.Course.Code,
                    CourseName = enr.Course.Name,
                    Credits = enr.Course.Credits,
                    TeacherName = enr.Course.Teacher != null 
                        ? $"{enr.Course.Teacher.Title ?? ""} {enr.Course.Teacher.FirstName} {enr.Course.Teacher.LastName}".Trim() 
                        : "Təyin olunmayıb",
                    TermName = enr.Course.Term.Name,
                    TotalLessons = total,
                    PresentCount = present,
                    AbsentCount = absent,
                    ExcusedCount = excused,
                    AttendancePercentage = percentage,
                    History = courseAttendances.Select(a => new StudentAttendanceItemDto
                    {
                        Id = a.Id,
                        LessonDate = a.LessonDate,
                        LessonTopic = a.LessonTopic,
                        Status = string.IsNullOrEmpty(a.Status) 
                            ? (a.IsPresent ? AttendanceStatus.Present : AttendanceStatus.Absent)
                            : a.Status,
                        Note = a.Note
                    }).ToList()
                });
            }

            return result;
        }

        private static (bool Allowed, string? ErrorMessage) VerifyCourseAccess(Course course, int currentUserId, string currentUserRole)
        {
            if (currentUserRole == UserRoles.Admin)
            {
                return (true, null);
            }

            if (currentUserRole == UserRoles.Teacher)
            {
                if (course.TeacherId == currentUserId)
                {
                    return (true, null);
                }
                return (false, "Siz yalnız sizə təhkim olunmuş fənlərin davamiyyətini idarə edə bilərsiniz.");
            }

            return (false, "Bu əməliyyat üçün icazəniz yoxdur.");
        }
    }
}
