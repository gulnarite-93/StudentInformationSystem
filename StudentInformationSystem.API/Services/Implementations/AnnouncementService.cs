using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.DTOs.Announcements;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Repositories;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Services.Implementations
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IRepository<Announcement> _announcementRepo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IRepository<Enrollment> _enrollmentRepo;

        public AnnouncementService(
            IRepository<Announcement> announcementRepo,
            IRepository<Course> courseRepo,
            IRepository<Enrollment> enrollmentRepo)
        {
            _announcementRepo = announcementRepo;
            _courseRepo = courseRepo;
            _enrollmentRepo = enrollmentRepo;
        }

        public async Task<IReadOnlyList<AnnouncementDto>> GetAnnouncementsAsync(int currentUserId, string currentUserRole)
        {
            var query = _announcementRepo.Query()
                .Include(a => a.Author)
                .Include(a => a.Course)
                .AsQueryable();

            List<Announcement> announcements;

            if (currentUserRole == UserRoles.Admin)
            {
                // Admin bütün elanları görür
                announcements = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }
            else if (currentUserRole == UserRoles.Teacher)
            {
                // Müəllim müəssisə üzrə ümumi elanları VƏ özünün tədris etdiyi fənlərin elanlarını görür
                announcements = await query
                    .Where(a => a.CourseId == null || (a.Course != null && a.Course.TeacherId == currentUserId))
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }
            else // Student
            {
                // Tələbənin yazıldığı aktiv kursların siyahısı
                var enrolledCourseIds = await _enrollmentRepo.Query()
                    .Where(e => e.StudentId == currentUserId && (e.Status == "Enrolled" || e.Status == "Completed"))
                    .Select(e => e.CourseId)
                    .ToListAsync();

                // Tələbə müəssisə üzrə ümumi elanları VƏ yalnız yazıldığı kursların elanlarını görür
                announcements = await query
                    .Where(a => a.CourseId == null || (a.CourseId != null && enrolledCourseIds.Contains(a.CourseId.Value)))
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }

            return announcements.Select(a => MapToDto(a, currentUserId, currentUserRole)).ToList();
        }

        public async Task<AnnouncementDto?> GetByIdAsync(int id, int currentUserId, string currentUserRole)
        {
            var announcement = await _announcementRepo.Query()
                .Include(a => a.Author)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null) return null;

            // Baxış icazəsinin yoxlanması
            if (currentUserRole == UserRoles.Admin)
            {
                return MapToDto(announcement, currentUserId, currentUserRole);
            }

            if (currentUserRole == UserRoles.Teacher)
            {
                if (announcement.CourseId == null || (announcement.Course != null && announcement.Course.TeacherId == currentUserId))
                {
                    return MapToDto(announcement, currentUserId, currentUserRole);
                }
                return null;
            }

            if (currentUserRole == UserRoles.Student)
            {
                if (announcement.CourseId == null)
                {
                    return MapToDto(announcement, currentUserId, currentUserRole);
                }

                var isEnrolled = await _enrollmentRepo.Query()
                    .AnyAsync(e => e.StudentId == currentUserId && e.CourseId == announcement.CourseId && (e.Status == "Enrolled" || e.Status == "Completed"));

                if (isEnrolled)
                {
                    return MapToDto(announcement, currentUserId, currentUserRole);
                }
                return null;
            }

            return null;
        }

        public async Task<(bool Success, string? ErrorMessage, int? AnnouncementId)> CreateAnnouncementAsync(
            AnnouncementCreateDto dto, int currentUserId, string currentUserRole)
        {
            if (currentUserRole == UserRoles.Student)
            {
                return (false, "Tələbələrin elan yaratmaq hüququ yoxdur.", null);
            }

            // 1. Müəssisə üzrə ümumi elan (CourseId == null)
            if (!dto.CourseId.HasValue)
            {
                if (currentUserRole != UserRoles.Admin)
                {
                    return (false, "Müəssisə üzrə ümumi elanları yalnız Sistem İnzibatçısı (Admin) yarada bilər. Müəllimlər yalnız öz fənləri üçün elan yarada bilər.", null);
                }
            }
            else // 2. Fənnə aid daxili elan (CourseId != null)
            {
                var course = await _courseRepo.GetByIdAsync(dto.CourseId.Value);
                if (course == null)
                {
                    return (false, "Göstərilən fənn tapılmadı.", null);
                }

                if (currentUserRole == UserRoles.Teacher && course.TeacherId != currentUserId)
                {
                    return (false, "Siz yalnız özünüzə təhkim olunmuş fənlər üçün elan yarada bilərsiniz.", null);
                }
            }

            var entity = new Announcement
            {
                Title = dto.Title.Trim(),
                Content = dto.Content.Trim(),
                AuthorId = currentUserId,
                CourseId = dto.CourseId,
                CreatedAt = DateTime.UtcNow
            };

            await _announcementRepo.AddAsync(entity);
            return (true, null, entity.Id);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAnnouncementAsync(
            int id, AnnouncementUpdateDto dto, int currentUserId, string currentUserRole)
        {
            var announcement = await _announcementRepo.Query()
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
            {
                return (false, "Elan tapılmadı.");
            }

            // İcazə yoxlanışı
            if (currentUserRole == UserRoles.Student)
            {
                return (false, "Tələbələrin elan redaktə etmək hüququ yoxdur.");
            }

            if (currentUserRole == UserRoles.Teacher)
            {
                if (announcement.AuthorId != currentUserId || announcement.Course == null || announcement.Course.TeacherId != currentUserId)
                {
                    return (false, "Siz yalnız öz fənniniz üçün yaratdığınız elanı redaktə edə bilərsiniz.");
                }

                if (!dto.CourseId.HasValue)
                {
                    return (false, "Müəllim elanı müəssisə üzrə ümumi elana çevirə bilməz.");
                }

                var targetCourse = await _courseRepo.GetByIdAsync(dto.CourseId.Value);
                if (targetCourse == null || targetCourse.TeacherId != currentUserId)
                {
                    return (false, "Seçilən fənn sizə təhkim olunmayıb.");
                }
            }
            else if (currentUserRole == UserRoles.Admin)
            {
                if (dto.CourseId.HasValue)
                {
                    var targetCourse = await _courseRepo.GetByIdAsync(dto.CourseId.Value);
                    if (targetCourse == null)
                    {
                        return (false, "Seçilən fənn tapılmadı.");
                    }
                }
            }

            announcement.Title = dto.Title.Trim();
            announcement.Content = dto.Content.Trim();
            announcement.CourseId = dto.CourseId;

            await _announcementRepo.UpdateAsync(announcement);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAnnouncementAsync(
            int id, int currentUserId, string currentUserRole)
        {
            var announcement = await _announcementRepo.Query()
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
            {
                return (false, "Elan tapılmadı.");
            }

            if (currentUserRole == UserRoles.Student)
            {
                return (false, "Tələbələrin elan silmək hüququ yoxdur.");
            }

            if (currentUserRole == UserRoles.Teacher)
            {
                if (announcement.AuthorId != currentUserId || announcement.Course == null || announcement.Course.TeacherId != currentUserId)
                {
                    return (false, "Siz yalnız öz fənniniz üçün yaratdığınız elanı silə bilərsiniz.");
                }
            }

            await _announcementRepo.DeleteAsync(announcement);
            return (true, null);
        }

        public async Task<IReadOnlyList<AnnouncementCourseOptionDto>> GetCreatableCoursesAsync(int currentUserId, string currentUserRole)
        {
            if (currentUserRole == UserRoles.Admin)
            {
                var courses = await _courseRepo.Query()
                    .OrderBy(c => c.Code)
                    .ToListAsync();

                return courses.Select(c => new AnnouncementCourseOptionDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name
                }).ToList();
            }

            if (currentUserRole == UserRoles.Teacher)
            {
                var courses = await _courseRepo.Query()
                    .Where(c => c.TeacherId == currentUserId)
                    .OrderBy(c => c.Code)
                    .ToListAsync();

                return courses.Select(c => new AnnouncementCourseOptionDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name
                }).ToList();
            }

            return Array.Empty<AnnouncementCourseOptionDto>();
        }

        private static AnnouncementDto MapToDto(Announcement a, int currentUserId, string currentUserRole)
        {
            bool canModify = false;

            if (currentUserRole == UserRoles.Admin)
            {
                canModify = true;
            }
            else if (currentUserRole == UserRoles.Teacher)
            {
                canModify = (a.AuthorId == currentUserId && a.Course != null && a.Course.TeacherId == currentUserId);
            }

            return new AnnouncementDto
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                CreatedAt = a.CreatedAt,
                AuthorId = a.AuthorId,
                AuthorName = a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}".Trim() : "Bilinmir",
                AuthorRole = a.Author?.Role ?? string.Empty,
                CourseId = a.CourseId,
                CourseCode = a.Course?.Code,
                CourseName = a.Course?.Name,
                CanEdit = canModify,
                CanDelete = canModify
            };
        }
    }
}
