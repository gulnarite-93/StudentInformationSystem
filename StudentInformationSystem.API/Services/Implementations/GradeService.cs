using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.DTOs.Grades;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Repositories;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Services.Implementations
{
    public class GradeService : IGradeService
    {
        private readonly IRepository<Grade> _gradeRepo;
        private readonly IRepository<Enrollment> _enrollmentRepo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Attendance> _attendanceRepo;
        private readonly IRepository<Announcement> _announcementRepo;
        private readonly IGpaService _gpaService;

        public GradeService(
            IRepository<Grade> gradeRepo,
            IRepository<Enrollment> enrollmentRepo,
            IRepository<Course> courseRepo,
            IRepository<User> userRepo,
            IRepository<Attendance> attendanceRepo,
            IRepository<Announcement> announcementRepo,
            IGpaService gpaService)
        {
            _gradeRepo = gradeRepo;
            _enrollmentRepo = enrollmentRepo;
            _courseRepo = courseRepo;
            _userRepo = userRepo;
            _attendanceRepo = attendanceRepo;
            _announcementRepo = announcementRepo;
            _gpaService = gpaService;
        }

        public async Task<IReadOnlyList<TeacherGradeCourseSummaryDto>> GetTeacherCoursesSummaryAsync(int teacherId, string role)
        {
            IQueryable<Course> query = _courseRepo.Query()
                .Include(c => c.Term)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Grade);

            if (role != UserRoles.Admin)
            {
                query = query.Where(c => c.TeacherId == teacherId);
            }

            var courses = await query
                .OrderBy(c => c.Code)
                .ToListAsync();

            return courses.Select(c =>
            {
                var activeEnrollments = c.Enrollments.Where(e => e.Status == "Enrolled" || e.Status == "Completed").ToList();
                return new TeacherGradeCourseSummaryDto
                {
                    CourseId = c.Id,
                    CourseCode = c.Code,
                    CourseName = c.Name,
                    Credits = c.Credits,
                    TermName = c.Term?.Name ?? string.Empty,
                    TotalEnrolled = activeEnrollments.Count,
                    GradedCount = activeEnrollments.Count(e => e.Grade != null)
                };
            }).ToList();
        }

        public async Task<CourseGradesDto?> GetCourseGradesAsync(int courseId, int currentUserId, string currentUserRole)
        {
            var course = await _courseRepo.Query()
                .Include(c => c.Teacher)
                .Include(c => c.Term)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return null;

            if (currentUserRole != UserRoles.Admin && course.TeacherId != currentUserId)
            {
                return null; // Müəllim yalnız öz kurslarına baxa bilər
            }

            var enrollments = await _enrollmentRepo.Query()
                .Include(e => e.Student)
                .Include(e => e.Grade)
                .Where(e => e.CourseId == courseId && (e.Status == "Enrolled" || e.Status == "Completed"))
                .OrderBy(e => e.Student.LastName)
                .ThenBy(e => e.Student.FirstName)
                .ToListAsync();

            var studentGrades = enrollments.Select(e => new CourseStudentGradeDto
            {
                EnrollmentId = e.Id,
                StudentId = e.StudentId,
                StudentName = $"{e.Student.FirstName} {e.Student.LastName}".Trim(),
                StudentNumber = e.Student.StudentNumber,
                Email = e.Student.Email,
                HasGrade = e.Grade != null,
                GradeId = e.Grade?.Id,
                MidtermScore = e.Grade?.MidtermScore,
                AssignmentScore = e.Grade?.AssignmentScore,
                FinalScore = e.Grade?.FinalScore,
                TotalScore = e.Grade?.TotalScore,
                LetterGrade = e.Grade?.LetterGrade,
                GradePoint = e.Grade?.GradePoint,
                Feedback = e.Grade?.Feedback,
                EvaluatedAt = e.Grade?.EvaluatedAt
            }).ToList();

            var graded = studentGrades.Where(s => s.HasGrade && s.TotalScore.HasValue).ToList();
            var avg = graded.Any() ? Math.Round(graded.Average(s => s.TotalScore!.Value), 2) : 0.0m;

            return new CourseGradesDto
            {
                CourseId = course.Id,
                CourseCode = course.Code,
                CourseName = course.Name,
                Credits = course.Credits,
                TeacherName = course.Teacher != null ? $"{course.Teacher.FirstName} {course.Teacher.LastName}".Trim() : "Təyin olunmayıb",
                TermName = course.Term?.Name ?? string.Empty,
                EnrolledStudentsCount = studentGrades.Count,
                GradedStudentsCount = graded.Count,
                AverageScore = avg,
                Students = studentGrades
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> SaveGradeAsync(GradeSaveDto dto, int currentUserId, string currentUserRole)
        {
            var enrollment = await _enrollmentRepo.Query()
                .Include(e => e.Course)
                .Include(e => e.Grade)
                .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

            if (enrollment == null)
            {
                return (false, "Tələbə qeydiyyatı (Enrollment) tapılmadı.");
            }

            if (currentUserRole != UserRoles.Admin && enrollment.Course.TeacherId != currentUserId)
            {
                return (false, "Siz yalnız öz kurslarınızın tələbələrinə qiymət daxil edə bilərsiniz.");
            }

            // Yekun balın (TotalScore) təyini:
            // Əgər TotalScore daxil edilibsə və ya komponentlərin cəmindən ibarətdirsə
            decimal totalScore;
            if (dto.TotalScore.HasValue && dto.TotalScore.Value > 0)
            {
                totalScore = dto.TotalScore.Value;
            }
            else
            {
                totalScore = (dto.MidtermScore ?? 0) + (dto.AssignmentScore ?? 0) + (dto.FinalScore ?? 0);
            }

            if (totalScore < 0) totalScore = 0;
            if (totalScore > 100) totalScore = 100;

            // 4.0 şkalası və hərf qiymətinin avtomatik hesablanması
            var (letter, gradePoint, _) = _gpaService.CalculateLetterGradeAndPoint(totalScore);

            if (enrollment.Grade == null)
            {
                var newGrade = new Grade
                {
                    EnrollmentId = enrollment.Id,
                    MidtermScore = dto.MidtermScore,
                    AssignmentScore = dto.AssignmentScore,
                    FinalScore = dto.FinalScore,
                    TotalScore = totalScore,
                    LetterGrade = letter,
                    GradePoint = gradePoint,
                    Feedback = dto.Feedback?.Trim(),
                    EvaluatedAt = DateTime.UtcNow
                };

                await _gradeRepo.AddAsync(newGrade);
            }
            else
            {
                enrollment.Grade.MidtermScore = dto.MidtermScore;
                enrollment.Grade.AssignmentScore = dto.AssignmentScore;
                enrollment.Grade.FinalScore = dto.FinalScore;
                enrollment.Grade.TotalScore = totalScore;
                enrollment.Grade.LetterGrade = letter;
                enrollment.Grade.GradePoint = gradePoint;
                enrollment.Grade.Feedback = dto.Feedback?.Trim();
                enrollment.Grade.EvaluatedAt = DateTime.UtcNow;

                await _gradeRepo.UpdateAsync(enrollment.Grade);
            }

            return (true, null);
        }

        public async Task<IReadOnlyList<GradeDto>> GetStudentGradesAsync(int studentId, int currentUserId, string currentUserRole)
        {
            if (currentUserRole == UserRoles.Student && currentUserId != studentId)
            {
                return Array.Empty<GradeDto>();
            }

            var enrollments = await _enrollmentRepo.Query()
                .Include(e => e.Course).ThenInclude(c => c.Term)
                .Include(e => e.Student)
                .Include(e => e.Grade)
                .Where(e => e.StudentId == studentId && e.Grade != null)
                .OrderByDescending(e => e.Course.Term.StartDate)
                .ThenBy(e => e.Course.Code)
                .ToListAsync();

            return enrollments.Select(e => new GradeDto
            {
                Id = e.Grade!.Id,
                EnrollmentId = e.Id,
                CourseId = e.CourseId,
                CourseCode = e.Course.Code,
                CourseName = e.Course.Name,
                Credits = e.Course.Credits,
                TermName = e.Course.Term?.Name ?? string.Empty,
                StudentId = e.StudentId,
                StudentName = $"{e.Student.FirstName} {e.Student.LastName}".Trim(),
                StudentNumber = e.Student.StudentNumber,
                MidtermScore = e.Grade.MidtermScore,
                AssignmentScore = e.Grade.AssignmentScore,
                FinalScore = e.Grade.FinalScore,
                TotalScore = e.Grade.TotalScore,
                LetterGrade = e.Grade.LetterGrade,
                GradePoint = e.Grade.GradePoint,
                Feedback = e.Grade.Feedback,
                EvaluatedAt = e.Grade.EvaluatedAt
            }).ToList();
        }

        public async Task<StudentTranscriptDto?> GetStudentTranscriptAsync(int studentId, int currentUserId, string currentUserRole)
        {
            if (currentUserRole == UserRoles.Student && currentUserId != studentId)
            {
                return null;
            }

            var student = await _userRepo.Query()
                .Include(u => u.Programme)
                    .ThenInclude(p => p != null ? p.Department : null)
                .FirstOrDefaultAsync(u => u.Id == studentId);

            if (student == null) return null;

            var enrollments = await _enrollmentRepo.Query()
                .Include(e => e.Course).ThenInclude(c => c.Term)
                .Include(e => e.Grade)
                .Where(e => e.StudentId == studentId && (e.Status == "Enrolled" || e.Status == "Completed"))
                .OrderBy(e => e.Course.Term.StartDate)
                .ThenBy(e => e.Course.Code)
                .ToListAsync();

            var transcript = new StudentTranscriptDto
            {
                StudentId = student.Id,
                FullName = $"{student.FirstName} {student.LastName}".Trim(),
                StudentNumber = student.StudentNumber,
                Email = student.Email,
                DepartmentName = student.Programme?.Department?.Name ?? "Qeyd olunmayıb",
                ProgrammeName = student.Programme?.Name ?? "Qeyd olunmayıb",
                AcademicYear = student.AcademicYear,
                GeneratedAt = DateTime.UtcNow
            };

            var gradedEnrollments = enrollments.Where(e => e.Grade != null).ToList();

            // Ümumi GPA hesablama: (GradePoint * Credits) / Credits
            var gpaInput = gradedEnrollments.Select(e => (e.Grade!.GradePoint, e.Course.Credits));
            transcript.Gpa = _gpaService.CalculateGpa(gpaInput);

            transcript.TotalCreditsAttempted = enrollments.Sum(e => e.Course.Credits);
            transcript.TotalCreditsCompleted = gradedEnrollments
                .Where(e => e.Grade!.TotalScore >= 51.00m)
                .Sum(e => e.Course.Credits);

            // Semestrlər üzrə qruplaşdırma
            var termGroups = enrollments.GroupBy(e => e.Course.TermId);

            foreach (var group in termGroups)
            {
                var first = group.First();
                var termName = first.Course.Term?.Name ?? "Naməlum Semestr";

                var termGraded = group.Where(e => e.Grade != null).ToList();
                var termGpa = _gpaService.CalculateGpa(termGraded.Select(e => (e.Grade!.GradePoint, e.Course.Credits)));

                var termDto = new TranscriptTermDto
                {
                    TermId = group.Key,
                    TermName = termName,
                    TermGpa = termGpa,
                    TermCredits = group.Sum(e => e.Course.Credits),
                    Courses = group.Select(e => new TranscriptCourseItemDto
                    {
                        CourseId = e.CourseId,
                        CourseCode = e.Course.Code,
                        CourseName = e.Course.Name,
                        Credits = e.Course.Credits,
                        TotalScore = e.Grade?.TotalScore ?? 0.0m,
                        LetterGrade = e.Grade?.LetterGrade ?? "-",
                        GradePoint = e.Grade?.GradePoint ?? 0.0m,
                        Status = (e.Grade != null && e.Grade.TotalScore >= 51.00m) ? "Məqbul" : (e.Grade != null ? "Kəsr" : "Davam edir")
                    }).ToList()
                };

                transcript.Terms.Add(termDto);
            }

            return transcript;
        }

        public async Task<StudentDashboardDto?> GetStudentDashboardAsync(int studentId, int currentUserId, string currentUserRole)
        {
            if (currentUserRole == UserRoles.Student && currentUserId != studentId)
            {
                return null;
            }

            var student = await _userRepo.Query()
                .Include(u => u.Programme)
                .FirstOrDefaultAsync(u => u.Id == studentId);

            if (student == null) return null;

            var enrollments = await _enrollmentRepo.Query()
                .Include(e => e.Course).ThenInclude(c => c.Teacher)
                .Include(e => e.Grade)
                .Where(e => e.StudentId == studentId && (e.Status == "Enrolled" || e.Status == "Completed"))
                .ToListAsync();

            var gradedEnrollments = enrollments.Where(e => e.Grade != null).ToList();
            var gpa = _gpaService.CalculateGpa(gradedEnrollments.Select(e => (e.Grade!.GradePoint, e.Course.Credits)));

            // Davamiyyət statistikası
            var attendances = await _attendanceRepo.Query()
                .Where(a => a.StudentId == studentId)
                .ToListAsync();

            var totalLessons = attendances.Count;
            var attended = attendances.Count(a => a.IsPresent || a.Note == "Excused" || a.Note == "İcazəli");
            var attendancePercentage = totalLessons > 0
                ? Math.Round((decimal)attended / totalLessons * 100, 1)
                : 100.0m;

            // Son elanlar (müəssisə üzrə və ya tələbənin kursları üzrə)
            var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToList();
            var announcements = await _announcementRepo.Query()
                .Include(a => a.Author)
                .Include(a => a.Course)
                .Where(a => a.CourseId == null || enrolledCourseIds.Contains(a.CourseId.Value))
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var announcementDtos = announcements.Select(a => new DashboardAnnouncementDto
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                AuthorName = a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}".Trim() : "Sistem",
                CourseName = a.Course?.Name,
                CreatedAt = a.CreatedAt
            }).ToList();

            var currentCourses = enrollments.Select(e => new DashboardCourseDto
            {
                CourseId = e.CourseId,
                CourseCode = e.Course.Code,
                CourseName = e.Course.Name,
                Credits = e.Course.Credits,
                TeacherName = e.Course.Teacher != null ? $"{e.Course.Teacher.FirstName} {e.Course.Teacher.LastName}".Trim() : "Təyin olunmayıb",
                HasGrade = e.Grade != null,
                TotalScore = e.Grade?.TotalScore,
                LetterGrade = e.Grade?.LetterGrade,
                GradePoint = e.Grade?.GradePoint
            }).ToList();

            return new StudentDashboardDto
            {
                StudentId = student.Id,
                FullName = $"{student.FirstName} {student.LastName}".Trim(),
                StudentNumber = student.StudentNumber,
                ProgrammeName = student.Programme?.Name ?? "Təyin olunmayıb",
                AcademicYear = student.AcademicYear,
                CurrentGpa = gpa,
                TotalCredits = enrollments.Sum(e => e.Course.Credits),
                EnrolledCoursesCount = enrollments.Count,
                AttendancePercentage = attendancePercentage,
                TotalLessons = totalLessons,
                AttendedLessons = attended,
                RecentAnnouncements = announcementDtos,
                CurrentCourses = currentCourses
            };
        }
    }
}
