using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
        {
            // Baza cədvəllərinin mövcudluğunu və miqrasiyaların tətbiqini təmin edirik
            await context.Database.MigrateAsync();

            if (await context.Users.AnyAsync())
            {
                // Əgər bazada istifadəçilər artıq varsa, lakin hələ qeydiyyatlar yoxdursa, ilkin qeydiyyatları əlavə edirik
                if (!await context.Enrollments.AnyAsync() && await context.Courses.AnyAsync())
                {
                    var s1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "student.aydan");
                    var s2 = await context.Users.FirstOrDefaultAsync(u => u.Username == "student.elmir");
                    var c1 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "IT101");
                    var c2 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "IT102");

                    if (s1 != null && s2 != null && c1 != null && c2 != null)
                    {
                        await context.Enrollments.AddRangeAsync(
                            new Enrollment { StudentId = s1.Id, CourseId = c1.Id, EnrollmentDate = DateTime.UtcNow, Status = "Enrolled" },
                            new Enrollment { StudentId = s1.Id, CourseId = c2.Id, EnrollmentDate = DateTime.UtcNow, Status = "Enrolled" },
                            new Enrollment { StudentId = s2.Id, CourseId = c2.Id, EnrollmentDate = DateTime.UtcNow, Status = "Enrolled" }
                        );
                        await context.SaveChangesAsync();
                    }
                }
                return;
            }

            // 1. Şöbələr (Departments)
            var csDept = new Department
            {
                Name = "Kompüter Elmləri və Mühəndislik",
                Code = "CS",
                Description = "Kompüter elmləri, proqram təminatı və İT sahələri üzrə şöbə."
            };

            var econDept = new Department
            {
                Name = "İqtisadiyyat və İdarəetmə",
                Code = "ECON",
                Description = "Biznesin idarə edilməsi və maliyyə ixtisasları."
            };

            await context.Departments.AddRangeAsync(csDept, econDept);
            await context.SaveChangesAsync();

            // 2. Proqramlar (Programmes)
            var itProg = new Programme
            {
                Name = "İnformasiya Texnologiyaları",
                Code = "IT",
                DepartmentId = csDept.Id
            };

            var ceProg = new Programme
            {
                Name = "Kompüter Mühəndisliyi",
                Code = "CE",
                DepartmentId = csDept.Id
            };

            var finProg = new Programme
            {
                Name = "Maliyyə və Mühasibat",
                Code = "FIN",
                DepartmentId = econDept.Id
            };

            await context.Programmes.AddRangeAsync(itProg, ceProg, finProg);
            await context.SaveChangesAsync();

            // 3. Akademik Dövrlər (Terms)
            var fallTerm = new Term
            {
                Name = "2026-2027 Payız Semestri",
                StartDate = new DateTime(2026, 9, 15),
                EndDate = new DateTime(2027, 1, 30),
                IsActive = true
            };

            var springTerm = new Term
            {
                Name = "2026-2027 Yaz Semestri",
                StartDate = new DateTime(2027, 2, 15),
                EndDate = new DateTime(2027, 6, 30),
                IsActive = false
            };

            await context.Terms.AddRangeAsync(fallTerm, springTerm);
            await context.SaveChangesAsync();

            // 4. İstifadəçilər (Admin, Müəllimlər, Tələbələr)
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@sis.edu.az",
                PasswordHash = passwordHasher.HashPassword("Admin123!"),
                FirstName = "Sistem",
                LastName = "İnzibatçısı",
                Role = UserRoles.Admin,
                CreatedAt = DateTime.UtcNow
            };

            var teacher1 = new User
            {
                Username = "teacher.rasim",
                Email = "rasim.eliyev@sis.edu.az",
                PasswordHash = passwordHasher.HashPassword("Teacher123!"),
                FirstName = "Rasim",
                LastName = "Əliyev",
                Role = UserRoles.Teacher,
                DepartmentId = csDept.Id,
                Title = "Dosent",
                CreatedAt = DateTime.UtcNow
            };

            var teacher2 = new User
            {
                Username = "teacher.leyla",
                Email = "leyla.qasimova@sis.edu.az",
                PasswordHash = passwordHasher.HashPassword("Teacher123!"),
                FirstName = "Leyla",
                LastName = "Qasımova",
                Role = UserRoles.Teacher,
                DepartmentId = csDept.Id,
                Title = "Baş müəllim",
                CreatedAt = DateTime.UtcNow
            };

            var student1 = new User
            {
                Username = "student.aydan",
                Email = "aydan.memmedova@sis.edu.az",
                PasswordHash = passwordHasher.HashPassword("Student123!"),
                FirstName = "Aydan",
                LastName = "Məmmədova",
                Role = UserRoles.Student,
                StudentNumber = "ST2026001",
                AcademicYear = 2,
                ProgrammeId = itProg.Id,
                CreatedAt = DateTime.UtcNow
            };

            var student2 = new User
            {
                Username = "student.elmir",
                Email = "elmir.huseynov@sis.edu.az",
                PasswordHash = passwordHasher.HashPassword("Student123!"),
                FirstName = "Elmir",
                LastName = "Hüseynov",
                Role = UserRoles.Student,
                StudentNumber = "ST2026002",
                AcademicYear = 2,
                ProgrammeId = itProg.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(adminUser, teacher1, teacher2, student1, student2);
            await context.SaveChangesAsync();

            // 5. Kurslar (Courses)
            var course1 = new Course
            {
                Code = "IT101",
                Name = "Proqramlaşdırmanın Əsasları (C#)",
                Credits = 6,
                Description = "C# dili, obyekt yönümlü proqramlaşdırma və əsas alqoritmlər.",
                ProgrammeId = itProg.Id,
                TermId = fallTerm.Id,
                TeacherId = teacher1.Id
            };

            var course2 = new Course
            {
                Code = "IT102",
                Name = "Verilənlər Bazası Sistemləri (SQL)",
                Credits = 5,
                Description = "Relyasiyalı verilənlər bazaları, SQL və relyasiya cədvəllərinin dizaynı.",
                ProgrammeId = itProg.Id,
                TermId = fallTerm.Id,
                TeacherId = teacher2.Id
            };

            var course3 = new Course
            {
                Code = "IT201",
                Name = "Veb Proqramlaşdırma (ASP.NET Core)",
                Credits = 6,
                Description = "Müasir veb tətbiqlər, REST API və MVC arxitekturası.",
                ProgrammeId = itProg.Id,
                TermId = fallTerm.Id,
                TeacherId = teacher1.Id
            };

            await context.Courses.AddRangeAsync(course1, course2, course3);
            await context.SaveChangesAsync();

            // 6. İlkin Tələbə Qeydiyyatları (Enrollments)
            var enr1 = new Enrollment { StudentId = student1.Id, CourseId = course1.Id, EnrollmentDate = DateTime.UtcNow, Status = "Enrolled" };
            var enr2 = new Enrollment { StudentId = student1.Id, CourseId = course2.Id, EnrollmentDate = DateTime.UtcNow, Status = "Enrolled" };
            var enr3 = new Enrollment { StudentId = student2.Id, CourseId = course2.Id, EnrollmentDate = DateTime.UtcNow, Status = "Enrolled" };

            await context.Enrollments.AddRangeAsync(enr1, enr2, enr3);
            await context.SaveChangesAsync();

            // 7. İlkin Elan (Ümumi müəssisə üzrə)
            var announcement = new Announcement
            {
                Title = "Yeni Tədris İliniz Mübarək!",
                Content = "Bütün müəllim heyətini və tələbələrimizi yeni akademik il münasibətilə təbrik edir, dərslərdə uğurlar arzulayırıq.",
                AuthorId = adminUser.Id,
                CourseId = null,
                CreatedAt = DateTime.UtcNow
            };

            await context.Announcements.AddAsync(announcement);
            await context.SaveChangesAsync();
        }
    }
}
