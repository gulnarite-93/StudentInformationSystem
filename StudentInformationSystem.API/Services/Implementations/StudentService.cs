using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.DTOs.Students;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Repositories;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Programme> _programmeRepo;
        private readonly IRepository<Enrollment> _enrollmentRepo;
        private readonly IRepository<Attendance> _attendanceRepo;
        private readonly IPasswordHasher _passwordHasher;

        public StudentService(
            IRepository<User> userRepo,
            IRepository<Programme> programmeRepo,
            IRepository<Enrollment> enrollmentRepo,
            IRepository<Attendance> attendanceRepo,
            IPasswordHasher passwordHasher)
        {
            _userRepo = userRepo;
            _programmeRepo = programmeRepo;
            _enrollmentRepo = enrollmentRepo;
            _attendanceRepo = attendanceRepo;
            _passwordHasher = passwordHasher;
        }

        public async Task<IReadOnlyList<StudentDto>> GetAllAsync(string? search = null, int? programmeId = null, int? academicYear = null)
        {
            var query = _userRepo.Query()
                .Include(u => u.Programme)
                    .ThenInclude(p => p!.Department)
                .Where(u => u.Role == UserRoles.Student);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(term) ||
                    u.LastName.ToLower().Contains(term) ||
                    (u.StudentNumber != null && u.StudentNumber.ToLower().Contains(term)) ||
                    u.Email.ToLower().Contains(term) ||
                    u.Username.ToLower().Contains(term));
            }

            if (programmeId.HasValue && programmeId.Value > 0)
            {
                query = query.Where(u => u.ProgrammeId == programmeId.Value);
            }

            if (academicYear.HasValue && academicYear.Value > 0)
            {
                query = query.Where(u => u.AcademicYear == academicYear.Value);
            }

            return await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new StudentDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    StudentNumber = u.StudentNumber,
                    PhoneNumber = u.PhoneNumber,
                    DateOfBirth = u.DateOfBirth,
                    AcademicYear = u.AcademicYear,
                    ProgrammeId = u.ProgrammeId,
                    ProgrammeName = u.Programme != null ? u.Programme.Name : null,
                    DepartmentName = u.Programme != null && u.Programme.Department != null ? u.Programme.Department.Name : null,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<StudentDto?> GetByIdAsync(int id)
        {
            return await _userRepo.Query()
                .Include(u => u.Programme)
                    .ThenInclude(p => p!.Department)
                .Where(u => u.Id == id && u.Role == UserRoles.Student)
                .Select(u => new StudentDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    StudentNumber = u.StudentNumber,
                    PhoneNumber = u.PhoneNumber,
                    DateOfBirth = u.DateOfBirth,
                    AcademicYear = u.AcademicYear,
                    ProgrammeId = u.ProgrammeId,
                    ProgrammeName = u.Programme != null ? u.Programme.Name : null,
                    DepartmentName = u.Programme != null && u.Programme.Department != null ? u.Programme.Department.Name : null,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<StudentProfileDto?> GetProfileByUserIdAsync(int userId)
        {
            var user = await _userRepo.Query()
                .Include(u => u.Programme)
                    .ThenInclude(p => p!.Department)
                .Include(u => u.Enrollments)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new StudentProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                StudentNumber = user.StudentNumber,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                AcademicYear = user.AcademicYear,
                ProgrammeId = user.ProgrammeId,
                ProgrammeName = user.Programme?.Name,
                DepartmentName = user.Programme?.Department?.Name,
                CreatedAt = user.CreatedAt,
                EnrolledCoursesCount = user.Enrollments.Count,
                CompletedCoursesCount = user.Enrollments.Count(e => e.Status == "Completed")
            };
        }

        public async Task<(bool Success, StudentDto? Student, string? ErrorMessage)> CreateAsync(StudentCreateDto dto)
        {
            var usernameExists = await _userRepo.Query().AnyAsync(u => u.Username.ToLower() == dto.Username.Trim().ToLower());
            if (usernameExists)
            {
                return (false, null, "Bu istifadəçi adı artıq mövcuddur.");
            }

            var emailExists = await _userRepo.Query().AnyAsync(u => u.Email.ToLower() == dto.Email.Trim().ToLower());
            if (emailExists)
            {
                return (false, null, "Bu e-poçt ünvanı ilə artıq qeydiyyatdan keçilib.");
            }

            var studentNumberExists = await _userRepo.Query().AnyAsync(u => u.StudentNumber != null && u.StudentNumber.ToLower() == dto.StudentNumber.Trim().ToLower());
            if (studentNumberExists)
            {
                return (false, null, "Bu tələbə nömrəsi artıq başqa tələbəyə təyin edilib.");
            }

            var programme = await _programmeRepo.Query().Include(p => p.Department).FirstOrDefaultAsync(p => p.Id == dto.ProgrammeId);
            if (programme == null)
            {
                return (false, null, "Seçilmiş təhsil proqramı tapılmadı.");
            }

            var student = new User
            {
                Username = dto.Username.Trim().ToLower(),
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Role = UserRoles.Student,
                StudentNumber = dto.StudentNumber.Trim().ToUpper(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                DateOfBirth = dto.DateOfBirth,
                AcademicYear = dto.AcademicYear,
                ProgrammeId = dto.ProgrammeId,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(student);

            var studentDto = new StudentDto
            {
                Id = student.Id,
                Username = student.Username,
                Email = student.Email,
                FirstName = student.FirstName,
                LastName = student.LastName,
                StudentNumber = student.StudentNumber,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                AcademicYear = student.AcademicYear,
                ProgrammeId = student.ProgrammeId,
                ProgrammeName = programme.Name,
                DepartmentName = programme.Department?.Name,
                CreatedAt = student.CreatedAt
            };

            return (true, studentDto, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, StudentUpdateDto dto)
        {
            var student = await _userRepo.Query().FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRoles.Student);
            if (student == null)
            {
                return (false, "Tələbə tapılmadı.");
            }

            var emailExists = await _userRepo.Query().AnyAsync(u => u.Id != id && u.Email.ToLower() == dto.Email.Trim().ToLower());
            if (emailExists)
            {
                return (false, "Bu e-poçt ünvanı artıq başqa istifadəçi tərəfindən istifadə olunur.");
            }

            var studentNumberExists = await _userRepo.Query().AnyAsync(u => u.Id != id && u.StudentNumber != null && u.StudentNumber.ToLower() == dto.StudentNumber.Trim().ToLower());
            if (studentNumberExists)
            {
                return (false, "Bu tələbə nömrəsi artıq başqa tələbəyə təyin edilib.");
            }

            var programme = await _programmeRepo.GetByIdAsync(dto.ProgrammeId);
            if (programme == null)
            {
                return (false, "Seçilmiş təhsil proqramı tapılmadı.");
            }

            student.Email = dto.Email.Trim().ToLower();
            student.FirstName = dto.FirstName.Trim();
            student.LastName = dto.LastName.Trim();
            student.StudentNumber = dto.StudentNumber.Trim().ToUpper();
            student.PhoneNumber = dto.PhoneNumber?.Trim();
            student.DateOfBirth = dto.DateOfBirth;
            student.AcademicYear = dto.AcademicYear;
            student.ProgrammeId = dto.ProgrammeId;

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                student.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
            }

            await _userRepo.UpdateAsync(student);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            var student = await _userRepo.Query().FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRoles.Student);
            if (student == null)
            {
                return (false, "Tələbə tapılmadı.");
            }

            var hasEnrollments = await _enrollmentRepo.Query().AnyAsync(e => e.StudentId == id);
            if (hasEnrollments)
            {
                return (false, "Bu tələbənin kurs qeydiyyatları olduğu üçün silinə bilməz.");
            }

            var hasAttendances = await _attendanceRepo.Query().AnyAsync(a => a.StudentId == id);
            if (hasAttendances)
            {
                return (false, "Bu tələbənin davamiyyət qeydləri olduğu üçün silinə bilməz.");
            }

            await _userRepo.DeleteAsync(student);
            return (true, null);
        }
    }
}
