using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.Entities;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
        {
            Console.WriteLine("--> [DbInitializer] Starting SeedAsync...");

            // 1. Baza cədvəllərinin mövcudluğu və miqrasiyalar
            try
            {
                await context.Database.MigrateAsync();
                Console.WriteLine("--> [DbInitializer] MigrateAsync succeeded.");
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 1801)
            {
                Console.WriteLine("--> [DbInitializer] Database already exists (Error 1801). Continuing with seeding...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> [DbInitializer] MigrateAsync warning: {ex.Message}. Continuing with seeding...");
            }

            // 2. Attendances cədvəlində Status sütununun mövcudluğu
            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('Attendances') AND name = 'Status'
                    )
                    BEGIN
                        ALTER TABLE Attendances ADD [Status] nvarchar(max) NOT NULL CONSTRAINT DF_Attendances_Status DEFAULT 'Present';
                    END
                ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> [DbInitializer] Raw SQL check warning: {ex.Message}");
            }

            // 3. Artıq olan əlavə məlumatların təmizlənməsi (Kiçildilmiş hədəf: 3 şöbə, 4 müəllim, 10 tələbə, 8 kurs)
            await PruneExtraRecordsAsync(context);

            // 4. Akademik Dövrlər (Terms)
            var fallTerm = await context.Terms.FirstOrDefaultAsync(t => t.Name == "2026-2027 Payız Semestri");
            if (fallTerm == null)
            {
                fallTerm = new Term
                {
                    Name = "2026-2027 Payız Semestri",
                    StartDate = new DateTime(2026, 9, 15),
                    EndDate = new DateTime(2027, 1, 30),
                    IsActive = true
                };
                await context.Terms.AddAsync(fallTerm);
            }

            var springTerm = await context.Terms.FirstOrDefaultAsync(t => t.Name == "2026-2027 Yaz Semestri");
            if (springTerm == null)
            {
                springTerm = new Term
                {
                    Name = "2026-2027 Yaz Semestri",
                    StartDate = new DateTime(2027, 2, 15),
                    EndDate = new DateTime(2027, 6, 30),
                    IsActive = false
                };
                await context.Terms.AddAsync(springTerm);
            }
            await context.SaveChangesAsync();
            Console.WriteLine("--> [DbInitializer] Terms seeded.");

            // 5. Şöbələr (Cəmi 3 şöbə: CS, ECON, DESIGN)
            var csDept = await GetOrCreateDepartmentAsync(context, "CS", "Kompüter Elmləri və Mühəndislik", "Kompüter elmləri, proqram təminatı və İT sahələri üzrə şöbə.");
            var econDept = await GetOrCreateDepartmentAsync(context, "ECON", "İqtisadiyyat və İdarəetmə", "Biznesin idarə edilməsi, maliyyə və menecment sahələri üzrə şöbə.");
            var designDept = await GetOrCreateDepartmentAsync(context, "DESIGN", "Dizayn və Rəqəmsal Media", "Qrafik dizayn, rəqəmsal media və interfeys dizaynı üzrə şöbə.");
            await context.SaveChangesAsync();
            Console.WriteLine("--> [DbInitializer] Departments seeded.");

            // 6. Proqramlar (Hər şöbədə 2 proqram, cəmi 6)
            var itProg = await GetOrCreateProgrammeAsync(context, "IT", "İnformasiya Texnologiyaları", csDept.Id);
            var ceProg = await GetOrCreateProgrammeAsync(context, "CE", "Kompüter Mühəndisliyi", csDept.Id);
            var finProg = await GetOrCreateProgrammeAsync(context, "FIN", "Maliyyə və Mühasibat", econDept.Id);
            var bbaProg = await GetOrCreateProgrammeAsync(context, "BBA", "Biznesin İdarə Edilməsi", econDept.Id);
            var desProg = await GetOrCreateProgrammeAsync(context, "DES", "Qrafik və Rəqəmsal Dizayn", designDept.Id);
            var uiuxProg = await GetOrCreateProgrammeAsync(context, "UIUX", "Veb və UI/UX Dizayn", designDept.Id);
            await context.SaveChangesAsync();
            Console.WriteLine("--> [DbInitializer] Programmes seeded.");

            // 7. İstifadəçilər
            // A. Admin (1 nəfər)
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@sis.edu.az",
                    PasswordHash = passwordHasher.HashPassword("Admin123!"),
                    FirstName = "Sistem",
                    LastName = "İnzibatçısı",
                    Role = UserRoles.Admin,
                    CreatedAt = DateTime.UtcNow
                };
                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }

            // B. Müəllimlər (Cəmi 4 müəllim, şifrələri: Teacher123!)
            var teacherRasim = await GetOrCreateTeacherAsync(context, passwordHasher, "teacher.rasim", "rasim.eliyev@sis.edu.az", "Rasim", "Əliyev", csDept.Id, "Dosent");
            var teacherLeyla = await GetOrCreateTeacherAsync(context, passwordHasher, "teacher.leyla", "leyla.qasimova@sis.edu.az", "Leyla", "Qasımova", csDept.Id, "Baş müəllim");
            var teacherMurad = await GetOrCreateTeacherAsync(context, passwordHasher, "teacher.murad", "murad.quliyev@sis.edu.az", "Murad", "Quliyev", econDept.Id, "Baş müəllim");
            var teacherGunay = await GetOrCreateTeacherAsync(context, passwordHasher, "teacher.gunay", "gunay.memmedova@sis.edu.az", "Günay", "Məmmədova", designDept.Id, "Dosent");
            await context.SaveChangesAsync();
            Console.WriteLine("--> [DbInitializer] Teachers seeded.");

            // C. Tələbələr (Cəmi 10 tələbə, şifrələri: Student123!)
            var sAydan = await GetOrCreateStudentAsync(context, passwordHasher, "student.aydan", "aydan.memmedova@sis.edu.az", "Aydan", "Məmmədova", "ST2026001", 2, itProg.Id);
            var sElmir = await GetOrCreateStudentAsync(context, passwordHasher, "student.elmir", "elmir.huseynov@sis.edu.az", "Elmir", "Hüseynov", "ST2026002", 2, itProg.Id);
            var sNicat = await GetOrCreateStudentAsync(context, passwordHasher, "student.nicat", "nicat.kerimov@sis.edu.az", "Nicat", "Kərimov", "ST2026003", 1, itProg.Id);
            var sNigar = await GetOrCreateStudentAsync(context, passwordHasher, "student.nigar", "nigar.eliyeva@sis.edu.az", "Nigar", "Əliyeva", "ST2026004", 2, ceProg.Id);
            var sKenan = await GetOrCreateStudentAsync(context, passwordHasher, "student.kenan", "kenan.hesenov@sis.edu.az", "Kənan", "Həsənov", "ST2026005", 3, ceProg.Id);
            var sLeyla = await GetOrCreateStudentAsync(context, passwordHasher, "student.leyla", "leyla.babayeva@sis.edu.az", "Leyla", "Babayeva", "ST2026006", 1, finProg.Id);
            var sOrxan = await GetOrCreateStudentAsync(context, passwordHasher, "student.orxan", "orxan.memmedov@sis.edu.az", "Orxan", "Məmmədov", "ST2026007", 2, finProg.Id);
            var sSevinc = await GetOrCreateStudentAsync(context, passwordHasher, "student.sevinc", "sevinc.ismayilova@sis.edu.az", "Sevinc", "İsmayılova", "ST2026008", 3, bbaProg.Id);
            var sGunel = await GetOrCreateStudentAsync(context, passwordHasher, "student.gunel", "gunel.qasimova@sis.edu.az", "Günel", "Qasımova", "ST2026009", 1, desProg.Id);
            var sFidan = await GetOrCreateStudentAsync(context, passwordHasher, "student.fidan", "fidan.mustafayeva@sis.edu.az", "Fidan", "Mustafayeva", "ST2026010", 2, uiuxProg.Id);
            await context.SaveChangesAsync();
            Console.WriteLine("--> [DbInitializer] Students seeded.");

            // 8. Kurslar (Cəmi 8 kurs, kreditləri 3-5, hər birinə müəllim təyin edilib)
            var cIT101 = await GetOrCreateCourseAsync(context, "IT101", "Proqramlaşdırmanın Əsasları (C#)", 4, "C# dili, obyekt yönümlü proqramlaşdırma və əsas alqoritmlər.", itProg.Id, fallTerm.Id, teacherRasim.Id);
            var cIT102 = await GetOrCreateCourseAsync(context, "IT102", "Verilənlər Bazası Sistemləri (SQL)", 4, "Relyasiyalı verilənlər bazaları, SQL sorğuları və verilənlərin normallaşdırılması.", itProg.Id, fallTerm.Id, teacherLeyla.Id);
            var cIT201 = await GetOrCreateCourseAsync(context, "IT201", "Veb Proqramlaşdırma (ASP.NET Core)", 5, "Müasir veb tətbiqlər, RESTful API və MVC memarlığı.", itProg.Id, fallTerm.Id, teacherRasim.Id);
            var cCE202 = await GetOrCreateCourseAsync(context, "CE202", "Kompüter Şəbəkələri və Əməliyyat Sistemləri", 4, "OSI modeli, TCP/IP protokolları, Linux mühiti və şəbəkə inzibatçılığı.", ceProg.Id, fallTerm.Id, teacherLeyla.Id);
            var cFIN101 = await GetOrCreateCourseAsync(context, "FIN101", "Maliyyə Uçotunun Əsasları", 3, "Maliyyə hesabatları, balans, mənfəət və zərər hesabatının təhlili.", finProg.Id, fallTerm.Id, teacherMurad.Id);
            var cBBA201 = await GetOrCreateCourseAsync(context, "BBA201", "Strateji Menecment və Liderlik", 3, "Təşkilati idarəetmə, SWOT təhlili, liderlik və komanda motivasiyası.", bbaProg.Id, fallTerm.Id, teacherMurad.Id);
            var cDES101 = await GetOrCreateCourseAsync(context, "DES101", "Qrafik Dizayn və Rəqəmsal Kompozisiya", 3, "Vizual kompozisiya, rəng nəzəriyyəsi, tipoqrafika və vektor qrafikası.", desProg.Id, fallTerm.Id, teacherGunay.Id);
            var cUIX201 = await GetOrCreateCourseAsync(context, "UIX201", "İstifadəçi İnterfeysi (UI/UX) Dizaynı", 4, "İstifadəçi tədqiqatı, wireframing, interaktiv prototip və dizayn sistemləri.", uiuxProg.Id, fallTerm.Id, teacherGunay.Id);
            await context.SaveChangesAsync();
            Console.WriteLine("--> [DbInitializer] Courses seeded.");

            // 9. Qeydiyyatlar (Enrollments - 10 tələbə)
            var enrollmentsToEnsure = new List<(User Student, Course Course)>
            {
                // Aydan (4 kurs)
                (sAydan, cIT101), (sAydan, cIT102), (sAydan, cIT201), (sAydan, cUIX201),
                // Elmir (4 kurs)
                (sElmir, cIT101), (sElmir, cIT102), (sElmir, cIT201), (sElmir, cCE202),
                // Nicat (3 kurs)
                (sNicat, cIT101), (sNicat, cIT102), (sNicat, cCE202),
                // Nigar (3 kurs)
                (sNigar, cIT101), (sNigar, cCE202), (sNigar, cIT201),
                // Kənan (3 kurs)
                (sKenan, cIT102), (sKenan, cIT201), (sKenan, cCE202),
                // Leyla B. (3 kurs)
                (sLeyla, cFIN101), (sLeyla, cBBA201), (sLeyla, cIT101),
                // Orxan (3 kurs)
                (sOrxan, cFIN101), (sOrxan, cBBA201), (sOrxan, cIT102),
                // Sevinc (4 kurs)
                (sSevinc, cFIN101), (sSevinc, cBBA201), (sSevinc, cDES101), (sSevinc, cUIX201),
                // Günel (3 kurs)
                (sGunel, cDES101), (sGunel, cUIX201), (sGunel, cIT101),
                // Fidan (3 kurs)
                (sFidan, cDES101), (sFidan, cUIX201), (sFidan, cIT201)
            };

            var existingEnrollments = await context.Enrollments
                .Select(e => new { e.Id, e.StudentId, e.CourseId })
                .ToListAsync();

            var enrollmentEntities = new List<Enrollment>();
            var newEnrollments = new List<Enrollment>();

            foreach (var (st, cr) in enrollmentsToEnsure)
            {
                var existing = existingEnrollments.FirstOrDefault(e => e.StudentId == st.Id && e.CourseId == cr.Id);
                if (existing != null)
                {
                    enrollmentEntities.Add(new Enrollment { Id = existing.Id, StudentId = existing.StudentId, CourseId = existing.CourseId });
                }
                else
                {
                    var enr = new Enrollment
                    {
                        StudentId = st.Id,
                        CourseId = cr.Id,
                        EnrollmentDate = DateTime.UtcNow.AddMonths(-2),
                        Status = "Enrolled"
                    };
                    newEnrollments.Add(enr);
                    enrollmentEntities.Add(enr);
                }
            }

            if (newEnrollments.Any())
            {
                await context.Enrollments.AddRangeAsync(newEnrollments);
                await context.SaveChangesAsync();
            }
            Console.WriteLine("--> [DbInitializer] Enrollments seeded.");

            // 10. Dərs Sessiyaları və Davamiyyət (Hər kurs üçün dəqiq 3 dərs sessiyası)
            var allCourses = new[] { cIT101, cIT102, cIT201, cCE202, cFIN101, cBBA201, cDES101, cUIX201 };

            var courseSessionTopics = new Dictionary<string, string[]>
            {
                ["IT101"] = new[] { "C# Sintaksisi, Dəyişənlər və Tiplər", "Şərt Operatorları və Dövrlər", "Obyektyönlü Proqramlaşdırma (OOP)" },
                ["IT102"] = new[] { "Relyasiyalı Modellər və Cədvəl Arxitekturası", "SQL Əsasları: SELECT, INSERT, UPDATE, DELETE", "JOIN Əməliyyatları və Aqreqat Funksiyalar" },
                ["IT201"] = new[] { "ASP.NET Core Memarlığı və Dependency Injection", "RESTful API Dizaynı və Controller Strukturu", "Entity Framework Core və Verilənlər Bazası" },
                ["CE202"] = new[] { "Kompüter Şəbəkələri və OSI Modeli", "TCP/IP Protokollar Yığını və Marşrutlaşdırma", "Linux Əməliyyat Sistemi və Proseslər" },
                ["FIN101"] = new[] { "Mühasibat Uçotuna Giriş və Hesablar Planı", "Maliyyə Hesabatları: Balans və Mənfəət", "Aktivlər və Öhdəliklərin Qiymətləndirilməsi" },
                ["BBA201"] = new[] { "Müasir Menecmentin Əsasları və Strukturlar", "Strateji Planlaşdırma və SWOT Analizi", "Liderlik Tərzləri və Komandanın Motivasiyası" },
                ["DES101"] = new[] { "Qrafik Dizaynın Əsasları: Xətt və Forma", "Tipoqrafika Qaydaları və Kompozisiya", "Vektor Qrafikası və İllüstrasiya" },
                ["UIX201"] = new[] { "İstifadəçi Təcrübəsi (UX) Tədqiqatı və Persona", "İnformasiya Arxitekturası və Wireframe", "UI Komponentləri və Dizayn Sistemləri" }
            };

            var sessionDates = new[]
            {
                new DateTime(2026, 10, 5),
                new DateTime(2026, 10, 12),
                new DateTime(2026, 10, 19)
            };

            var existingAttendanceKeys = new HashSet<(int CourseId, int StudentId, DateTime Date)>(
                await context.Attendances.Select(a => ValueTuple.Create(a.CourseId, a.StudentId, a.LessonDate.Date)).ToListAsync()
            );

            var attendancesToAdd = new List<Attendance>();

            foreach (var course in allCourses)
            {
                var enrolledStudentIds = enrollmentEntities
                    .Where(e => e.CourseId == course.Id)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .ToList();

                var topics = courseSessionTopics[course.Code];

                for (int sIndex = 0; sIndex < 3; sIndex++)
                {
                    var lessonDate = sessionDates[sIndex].AddDays(course.Id % 5);
                    var topic = topics[sIndex];

                    foreach (var studentId in enrolledStudentIds)
                    {
                        var key = (course.Id, studentId, lessonDate.Date);
                        if (!existingAttendanceKeys.Contains(key))
                        {
                            string status;
                            bool isPresent;
                            string? note;

                            int pattern = (studentId * 7 + sIndex * 13 + course.Id) % 10;
                            if (pattern == 0)
                            {
                                status = AttendanceStatus.Absent;
                                isPresent = false;
                                note = "Üzrsüz qayıb";
                            }
                            else if (pattern == 1)
                            {
                                status = AttendanceStatus.Excused;
                                isPresent = false;
                                note = "Xəstəlik arayışı təqdim olundu";
                            }
                            else
                            {
                                status = AttendanceStatus.Present;
                                isPresent = true;
                                note = pattern == 2 ? "Dərsdə fəal iştirak etdi" : null;
                            }

                            attendancesToAdd.Add(new Attendance
                            {
                                CourseId = course.Id,
                                StudentId = studentId,
                                LessonDate = lessonDate,
                                LessonTopic = topic,
                                Status = status,
                                IsPresent = isPresent,
                                Note = note
                            });
                            existingAttendanceKeys.Add(key);
                        }
                    }
                }
            }

            if (attendancesToAdd.Any())
            {
                await context.Attendances.AddRangeAsync(attendancesToAdd);
                await context.SaveChangesAsync();
            }
            Console.WriteLine("--> [DbInitializer] Attendances seeded.");

            // 11. Qiymətlər (45-100 aralığında, GPA fərqliliyi təmin edilir)
            var gradesDataset = new Dictionary<(string Username, string CourseCode), (decimal Midterm, decimal Assignment, decimal Final, string Feedback)>
            {
                // student.aydan
                { ("student.aydan", "IT101"), (28m, 18m, 46m, "C# dili və obyekt yönümlü proqramlaşdırmanı əla mənimsəyib.") },
                { ("student.aydan", "IT102"), (25m, 17m, 42m, "SQL sorğuları və verilənlər bazası layihəsini yaxşı icra etdi.") },
                { ("student.aydan", "IT201"), (22m, 15m, 38m, "Veb proqramlaşdırma üzrə praktiki tapşırıqları vaxtında təhvil verdi.") },
                { ("student.aydan", "UIX201"), (29m, 19m, 47m, "UI dizayn sistemi və figma prototipini yüksək səviyyədə hazırladı.") },

                // student.elmir
                { ("student.elmir", "IT101"), (27m, 18m, 43m, "Alqoritmlər və məntiqi düşünmə bacarığı yüksəkdir.") },
                { ("student.elmir", "IT102"), (27m, 19m, 45m, "Verilənlər bazası normallaşdırılması və mürəkkəb JOIN-lərdə çox fəaldır.") },
                { ("student.elmir", "IT201"), (23m, 16m, 39m, "ASP.NET Core API arxitekturasını uğurla tətbiq etdi.") },
                { ("student.elmir", "CE202"), (26m, 17m, 42m, "Linux və şəbəkə konfiqurasiyaları üzrə laboratoriya işlərini yaxşı müdafiə etdi.") },

                // student.nicat (Yüksək nəticəli tələbə - A)
                { ("student.nicat", "IT101"), (30m, 20m, 47m, "Kurs üzrə nümunəvi bilik və mükəmməl kodlaşdırma tərzi nümayiş etdirdi.") },
                { ("student.nicat", "IT102"), (28m, 19m, 45m, "Tranzaksiyalar və indeksləmə üzrə dərindən biliklərə malikdir.") },
                { ("student.nicat", "CE202"), (29m, 19m, 46m, "Şəbəkə protokollarını dərindən mənimsəyib.") },

                // student.nigar
                { ("student.nigar", "IT101"), (24m, 17m, 42m, "C# sintaksisi və metodların qurulmasını yaxşı bilir.") },
                { ("student.nigar", "CE202"), (22m, 16m, 38m, "Kompüter şəbəkələri üzrə nəzəriyyəni yaxşı qavrayıb.") },
                { ("student.nigar", "IT201"), (26m, 18m, 44m, "Veb layihəsini vaxtında təhvil verdi.") },

                // student.kenan
                { ("student.kenan", "IT102"), (21m, 15m, 36m, "SQL sorğularında bəzi səhvlər olsa da, ümumi bilikləri kafidir.") },
                { ("student.kenan", "IT201"), (19m, 14m, 32m, "Veb tətbiqlərdə daha çox praktika tələb olunur.") },
                { ("student.kenan", "CE202"), (24m, 16m, 41m, "Şəbəkə protokollarını yaxşı mənimsəyib.") },

                // student.leyla (Əlaçı tələbə)
                { ("student.leyla", "FIN101"), (29m, 19m, 46m, "Maliyyə hesabatlarının təhlili üzrə yüksək səviyyəli bilik nümayiş etdirdi.") },
                { ("student.leyla", "BBA201"), (27m, 18m, 44m, "Strateji idarəetmə və biznes keyslərinin təhlilində çox fəal idi.") },
                { ("student.leyla", "IT101"), (28m, 18m, 46m, "Proqramlaşdırma əsaslarından yüksək nəticə göstərdi.") },

                // student.orxan (Orta nəticə)
                { ("student.orxan", "FIN101"), (21m, 15m, 38m, "Mühasibat uçotunun əsas anlayışlarını qavrayıb.") },
                { ("student.orxan", "BBA201"), (22m, 15m, 39m, "Menecment dərslərində iştirakı yaxşıdır.") },
                { ("student.orxan", "IT102"), (20m, 14m, 36m, "Verilənlər bazası üzrə tapşırıqları təhvil verib.") },

                // student.sevinc
                { ("student.sevinc", "FIN101"), (26m, 18m, 44m, "Maliyyə uçotunda praktiki məsələləri sərbəst həll etdi.") },
                { ("student.sevinc", "BBA201"), (28m, 18m, 46m, "Liderlik və komanda idarəçiliyi təqdimatını əla təqdim etdi.") },
                { ("student.sevinc", "DES101"), (25m, 18m, 42m, "Qrafik kompozisiyası və rəng ahəngdarlığı zövqlüdür.") },
                { ("student.sevinc", "UIX201"), (27m, 18m, 45m, "İstifadəçi tədqiqatı hesabatını dəqiqliklə hazırladı.") },

                // student.gunel
                { ("student.gunel", "DES101"), (29m, 19m, 47m, "Qrafik dizayn üzrə portfolio işləri yüksək peşəkarlıqla yerinə yetirilib.") },
                { ("student.gunel", "UIX201"), (28m, 19m, 45m, "İnterfeys komponentləri və istifadəçi axınlarını səliqəli qurdu.") },
                { ("student.gunel", "IT101"), (27m, 18m, 44m, "Veb sahəsində kodlaşdırma biliklərini uğurla nümayiş etdirdi.") },

                // student.fidan
                { ("student.fidan", "DES101"), (27m, 19m, 45m, "Brendinq və loqotip dizaynı layihəsini əla hazırladı.") },
                { ("student.fidan", "UIX201"), (26m, 17m, 43m, "Mobil tətbiq interfeysi üzrə yaxşı prototip qurdu.") },
                { ("student.fidan", "IT201"), (28m, 19m, 46m, "Frontend və backend inteqrasiyasını mükəmməl qurdu.") }
            };

            var existingGradesEnrollmentIds = new HashSet<int>(
                await context.Grades.Select(g => g.EnrollmentId).ToListAsync()
            );

            var gradesToAdd = new List<Grade>();
            var allUsersDict = await context.Users.ToDictionaryAsync(u => u.Id, u => u.Username);
            var allCoursesDict = await context.Courses.ToDictionaryAsync(c => c.Id, c => c.Code);

            foreach (var enr in enrollmentEntities)
            {
                if (!existingGradesEnrollmentIds.Contains(enr.Id) &&
                    allUsersDict.TryGetValue(enr.StudentId, out var uname) &&
                    allCoursesDict.TryGetValue(enr.CourseId, out var ccode))
                {
                    var key = (uname, ccode);
                    if (gradesDataset.TryGetValue(key, out var gradeData))
                    {
                        var total = gradeData.Midterm + gradeData.Assignment + gradeData.Final;
                        var (letter, point) = GetLetterAndPoint(total);

                        gradesToAdd.Add(new Grade
                        {
                            EnrollmentId = enr.Id,
                            MidtermScore = gradeData.Midterm,
                            AssignmentScore = gradeData.Assignment,
                            FinalScore = gradeData.Final,
                            TotalScore = total,
                            LetterGrade = letter,
                            GradePoint = point,
                            Feedback = gradeData.Feedback,
                            EvaluatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 15))
                        });
                        existingGradesEnrollmentIds.Add(enr.Id);
                    }
                }
            }

            if (gradesToAdd.Any())
            {
                await context.Grades.AddRangeAsync(gradesToAdd);
                await context.SaveChangesAsync();
            }
            Console.WriteLine("--> [DbInitializer] Grades seeded.");

            // 12. Elanlar (3 elan: 2 müəssisə üzrə, 1 kursa aid)
            var announcementList = new List<(string Title, string Content, int AuthorId, int? CourseId)>
            {
                (
                    "2026-2027-ci Tədris İlinin Başlanması və Tələbə Qeydiyyatı",
                    "Hörmətli müəllimlər və tələbələr! 2026-2027-ci tədris ili münasibətilə hər kəsi təbrik edir, akademik fəaliyyətinizdə yeni nailiyyətlər arzulayırıq. Semestr üzrə kurs seçimləri və qeydiyyat prosesi aktivdir.",
                    adminUser.Id,
                    null
                ),
                (
                    "Elektron Kitabxana və Beynəlxalq Elmi Bazalara Giriş",
                    "Universitetimizin bütün tələbə və akademik heyəti üçün IEEE Xplore, ScienceDirect və SpringerLink elektron elmi bazalarına ödənişsiz giriş imkanı təmin edilmişdir. Şəxsi tələbə profili vasitəsilə resurslardan yararlana bilərsiniz.",
                    adminUser.Id,
                    null
                ),
                (
                    "Laboratoriya İşlərinin Qrafiki və Tələblər",
                    "Hörmətli tələbələr, IT101 kursu üzrə laboratoriya tapşırıqları portalda yerləşdirilmişdir. Hər bir tələbə tapşırıqları vaxtında təhvil verməlidir.",
                    teacherRasim.Id,
                    cIT101.Id
                )
            };

            foreach (var a in announcementList)
            {
                var exists = await context.Announcements.AnyAsync(an => an.Title == a.Title && an.CourseId == a.CourseId);
                if (!exists)
                {
                    await context.Announcements.AddAsync(new Announcement
                    {
                        Title = a.Title,
                        Content = a.Content,
                        AuthorId = a.AuthorId,
                        CourseId = a.CourseId,
                        CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 10))
                    });
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("--> [DbInitializer] Announcements seeded. Completed successfully!");
        }

        #region Helper Metodlar (İdempotent Yaratma və Təmizləmə)

        private static async Task PruneExtraRecordsAsync(AppDbContext context)
        {
            try
            {
                var extraCourseCodes = new[] { "MTH101", "DS201" };
                var extraUsernames = new[] { "teacher.kamran", "student.tural", "student.kamran", "student.samir", "student.aygun", "student.elvin" };
                var extraProgCodes = new[] { "AM", "DS" };
                var extraDeptCodes = new[] { "MATH" };

                bool needsPruning = await context.Courses.AnyAsync(c => extraCourseCodes.Contains(c.Code)) ||
                                    await context.Users.AnyAsync(u => extraUsernames.Contains(u.Username)) ||
                                    await context.Programmes.AnyAsync(p => extraProgCodes.Contains(p.Code)) ||
                                    await context.Departments.AnyAsync(d => extraDeptCodes.Contains(d.Code)) ||
                                    await context.Attendances.AnyAsync(a => a.LessonDate >= new DateTime(2026, 10, 25));

                if (!needsPruning) return;

                Console.WriteLine("--> [DbInitializer] Pruning extra records for lightweight database...");

                // 1. Grades
                var extraGrades = await context.Grades
                    .Where(g => extraCourseCodes.Contains(g.Enrollment.Course.Code) ||
                                extraUsernames.Contains(g.Enrollment.Student.Username))
                    .ToListAsync();
                if (extraGrades.Any()) context.Grades.RemoveRange(extraGrades);

                // 2. Attendances
                var extraAttendances = await context.Attendances
                    .Where(a => extraCourseCodes.Contains(a.Course.Code) ||
                                extraUsernames.Contains(a.Student.Username) ||
                                a.LessonDate >= new DateTime(2026, 10, 25))
                    .ToListAsync();
                if (extraAttendances.Any()) context.Attendances.RemoveRange(extraAttendances);

                // 3. Announcements
                var extraAnnouncements = await context.Announcements
                    .Where(an => (an.Course != null && extraCourseCodes.Contains(an.Course.Code)) ||
                                 extraUsernames.Contains(an.Author.Username))
                    .ToListAsync();
                if (extraAnnouncements.Any()) context.Announcements.RemoveRange(extraAnnouncements);

                // 4. Enrollments
                var extraEnrollments = await context.Enrollments
                    .Where(e => extraCourseCodes.Contains(e.Course.Code) ||
                                extraUsernames.Contains(e.Student.Username))
                    .ToListAsync();
                if (extraEnrollments.Any()) context.Enrollments.RemoveRange(extraEnrollments);

                await context.SaveChangesAsync();

                // 5. Courses
                var extraCourses = await context.Courses.Where(c => extraCourseCodes.Contains(c.Code)).ToListAsync();
                if (extraCourses.Any()) context.Courses.RemoveRange(extraCourses);

                // 6. Users
                var extraUsers = await context.Users.Where(u => extraUsernames.Contains(u.Username)).ToListAsync();
                if (extraUsers.Any()) context.Users.RemoveRange(extraUsers);

                await context.SaveChangesAsync();

                // 7. Programmes
                var extraProgrammes = await context.Programmes.Where(p => extraProgCodes.Contains(p.Code)).ToListAsync();
                if (extraProgrammes.Any()) context.Programmes.RemoveRange(extraProgrammes);

                // 8. Departments
                var extraDepts = await context.Departments.Where(d => extraDeptCodes.Contains(d.Code)).ToListAsync();
                if (extraDepts.Any()) context.Departments.RemoveRange(extraDepts);

                await context.SaveChangesAsync();
                Console.WriteLine("--> [DbInitializer] Pruning completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> [DbInitializer] Pruning warning: {ex.Message}");
            }
        }

        private static async Task<Department> GetOrCreateDepartmentAsync(AppDbContext context, string code, string name, string description)
        {
            var dept = await context.Departments.FirstOrDefaultAsync(d => d.Code == code);
            if (dept == null)
            {
                dept = new Department
                {
                    Code = code,
                    Name = name,
                    Description = description
                };
                await context.Departments.AddAsync(dept);
            }
            return dept;
        }

        private static async Task<Programme> GetOrCreateProgrammeAsync(AppDbContext context, string code, string name, int departmentId)
        {
            var prog = await context.Programmes.FirstOrDefaultAsync(p => p.Code == code);
            if (prog == null)
            {
                prog = new Programme
                {
                    Code = code,
                    Name = name,
                    DepartmentId = departmentId
                };
                await context.Programmes.AddAsync(prog);
            }
            return prog;
        }

        private static async Task<User> GetOrCreateTeacherAsync(AppDbContext context, IPasswordHasher passwordHasher, string username, string email, string firstName, string lastName, int departmentId, string title)
        {
            var teacher = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (teacher == null)
            {
                teacher = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = passwordHasher.HashPassword("Teacher123!"),
                    FirstName = firstName,
                    LastName = lastName,
                    Role = UserRoles.Teacher,
                    DepartmentId = departmentId,
                    Title = title,
                    CreatedAt = DateTime.UtcNow
                };
                await context.Users.AddAsync(teacher);
            }
            return teacher;
        }

        private static async Task<User> GetOrCreateStudentAsync(AppDbContext context, IPasswordHasher passwordHasher, string username, string email, string firstName, string lastName, string studentNumber, int academicYear, int programmeId)
        {
            var student = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (student == null)
            {
                student = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = passwordHasher.HashPassword("Student123!"),
                    FirstName = firstName,
                    LastName = lastName,
                    Role = UserRoles.Student,
                    StudentNumber = studentNumber,
                    AcademicYear = academicYear,
                    ProgrammeId = programmeId,
                    CreatedAt = DateTime.UtcNow
                };
                await context.Users.AddAsync(student);
            }
            return student;
        }

        private static async Task<Course> GetOrCreateCourseAsync(AppDbContext context, string code, string name, int credits, string description, int programmeId, int termId, int teacherId)
        {
            var course = await context.Courses.FirstOrDefaultAsync(c => c.Code == code);
            if (course == null)
            {
                course = new Course
                {
                    Code = code,
                    Name = name,
                    Credits = credits,
                    Description = description,
                    ProgrammeId = programmeId,
                    TermId = termId,
                    TeacherId = teacherId
                };
                await context.Courses.AddAsync(course);
            }
            else
            {
                if (course.TeacherId != teacherId) course.TeacherId = teacherId;
                if (course.Credits != credits) course.Credits = credits;
            }
            return course;
        }

        private static (string Letter, decimal Point) GetLetterAndPoint(decimal total)
        {
            if (total >= 91.00m) return ("A", 4.00m);
            if (total >= 81.00m) return ("B", 3.00m);
            if (total >= 71.00m) return ("C", 2.00m);
            if (total >= 61.00m) return ("D", 1.50m);
            if (total >= 51.00m) return ("E", 1.00m);
            return ("F", 0.00m);
        }

        #endregion
    }
}
