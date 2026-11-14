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

            // 1. Baza cədvəllərinin mövcudluğunu və miqrasiyaların tətbiqini təmin edirik
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

            // 2. Attendances cədvəlində Status sütununun mövcudluğunu birbaşa SQL ilə zəmanətə alırıq
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
                Console.WriteLine("--> [DbInitializer] Attendance Status column verified.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> [DbInitializer] Raw SQL check warning: {ex.Message}");
            }

            // 3. Akademik Dövrlər (Terms)
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
                await context.SaveChangesAsync();
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
                await context.SaveChangesAsync();
            }
            Console.WriteLine("--> [DbInitializer] Terms seeded.");

            // 4. Şöbələr (Departments - cəmi 4 şöbə)
            var csDept = await GetOrCreateDepartmentAsync(context, "CS", "Kompüter Elmləri və Mühəndislik", "Kompüter elmləri, proqram təminatı və İT sahələri üzrə şöbə.");
            var econDept = await GetOrCreateDepartmentAsync(context, "ECON", "İqtisadiyyat və İdarəetmə", "Biznesin idarə edilməsi, maliyyə və menecment sahələri üzrə şöbə.");
            var designDept = await GetOrCreateDepartmentAsync(context, "DESIGN", "Dizayn və Rəqəmsal Media", "Qrafik dizayn, rəqəmsal media və interfeys dizaynı üzrə şöbə.");
            var mathDept = await GetOrCreateDepartmentAsync(context, "MATH", "Tətbiqi Riyaziyyat və Kibertəhlükəsizlik", "Riyaziyyat, kriptoqrafiya və məlumat təhlili şöbəsi.");
            Console.WriteLine("--> [DbInitializer] Departments seeded.");

            // 5. Proqramlar (Programmes - hər şöbədə 2 proqram, cəmi 8)
            var itProg = await GetOrCreateProgrammeAsync(context, "IT", "İnformasiya Texnologiyaları", csDept.Id);
            var ceProg = await GetOrCreateProgrammeAsync(context, "CE", "Kompüter Mühəndisliyi", csDept.Id);
            var finProg = await GetOrCreateProgrammeAsync(context, "FIN", "Maliyyə və Mühasibat", econDept.Id);
            var bbaProg = await GetOrCreateProgrammeAsync(context, "BBA", "Biznesin İdarə Edilməsi", econDept.Id);
            var desProg = await GetOrCreateProgrammeAsync(context, "DES", "Qrafik və Rəqəmsal Dizayn", designDept.Id);
            var uiuxProg = await GetOrCreateProgrammeAsync(context, "UIUX", "Veb və UI/UX Dizayn", designDept.Id);
            var amProg = await GetOrCreateProgrammeAsync(context, "AM", "Tətbiqi Riyaziyyat", mathDept.Id);
            var dsProg = await GetOrCreateProgrammeAsync(context, "DS", "Məlumat Elmi və Süni İntellekt", mathDept.Id);
            Console.WriteLine("--> [DbInitializer] Programmes seeded.");

            // 6. İstifadəçilər
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

            // B. Müəllimlər (cəmi 5 nəfər, hamısının şifrəsi: Teacher123!)
            var teacherRasim = await GetOrCreateTeacherAsync(context, passwordHasher, "teacher.rasim", "rasim.eliyev@sis.edu.az", "Rasim", "Əliyev", csDept.Id, "Dosent");
            var teacherLeyla = await GetOrCreateTeacherAsync(context, passwordHasher, "teacher.leyla", "leyla.qasimova@sis.edu.az", "Leyla", "Qasımova", csDept.Id, "Baş müəllim");
            var teacherMurad = await GetOrCreateTeacherAsync(context, passwordHasher, "teacher.murad", "murad.quliyev@sis.edu.az", "Murad", "Quliyev", econDept.Id, "Baş müəllim");
            var teacherGunay = await GetOrCreateTeacherAsync(context, passwordHasher, "teacher.gunay", "gunay.memmedova@sis.edu.az", "Günay", "Məmmədova", designDept.Id, "Dosent");
            var teacherKamran = await GetOrCreateTeacherAsync(context, passwordHasher, "teacher.kamran", "kamran.rzayev@sis.edu.az", "Kamran", "Rzayev", mathDept.Id, "Professor");
            Console.WriteLine("--> [DbInitializer] Teachers seeded.");

            // C. Tələbələr (cəmi 15 nəfər, hamısının şifrəsi: Student123!)
            var sAydan = await GetOrCreateStudentAsync(context, passwordHasher, "student.aydan", "aydan.memmedova@sis.edu.az", "Aydan", "Məmmədova", "ST2026001", 2, itProg.Id);
            var sElmir = await GetOrCreateStudentAsync(context, passwordHasher, "student.elmir", "elmir.huseynov@sis.edu.az", "Elmir", "Hüseynov", "ST2026002", 2, itProg.Id);
            var sNicat = await GetOrCreateStudentAsync(context, passwordHasher, "student.nicat", "nicat.kerimov@sis.edu.az", "Nicat", "Kərimov", "ST2026003", 1, itProg.Id);
            var sNigar = await GetOrCreateStudentAsync(context, passwordHasher, "student.nigar", "nigar.eliyeva@sis.edu.az", "Nigar", "Əliyeva", "ST2026004", 2, ceProg.Id);
            var sKenan = await GetOrCreateStudentAsync(context, passwordHasher, "student.kenan", "kenan.hesenov@sis.edu.az", "Kənan", "Həsənov", "ST2026005", 3, ceProg.Id);
            var sLeyla = await GetOrCreateStudentAsync(context, passwordHasher, "student.leyla", "leyla.babayeva@sis.edu.az", "Leyla", "Babayeva", "ST2026006", 1, finProg.Id);
            var sOrxan = await GetOrCreateStudentAsync(context, passwordHasher, "student.orxan", "orxan.memmedov@sis.edu.az", "Orxan", "Məmmədov", "ST2026007", 2, finProg.Id);
            var sSevinc = await GetOrCreateStudentAsync(context, passwordHasher, "student.sevinc", "sevinc.ismayilova@sis.edu.az", "Sevinc", "İsmayılova", "ST2026008", 3, bbaProg.Id);
            var sTural = await GetOrCreateStudentAsync(context, passwordHasher, "student.tural", "tural.pasayev@sis.edu.az", "Tural", "Paşayev", "ST2026009", 4, bbaProg.Id);
            var sGunel = await GetOrCreateStudentAsync(context, passwordHasher, "student.gunel", "gunel.qasimova@sis.edu.az", "Günel", "Qasımova", "ST2026010", 1, desProg.Id);
            var sKamran = await GetOrCreateStudentAsync(context, passwordHasher, "student.kamran", "kamran.ceferov@sis.edu.az", "Kamran", "Cəfərov", "ST2026011", 2, uiuxProg.Id);
            var sFidan = await GetOrCreateStudentAsync(context, passwordHasher, "student.fidan", "fidan.mustafayeva@sis.edu.az", "Fidan", "Mustafayeva", "ST2026012", 3, uiuxProg.Id);
            var sSamir = await GetOrCreateStudentAsync(context, passwordHasher, "student.samir", "samir.veliyev@sis.edu.az", "Samir", "Vəliyev", "ST2026013", 1, amProg.Id);
            var sAygun = await GetOrCreateStudentAsync(context, passwordHasher, "student.aygun", "aygun.rehimova@sis.edu.az", "Aygün", "Rəhimova", "ST2026014", 2, dsProg.Id);
            var sElvin = await GetOrCreateStudentAsync(context, passwordHasher, "student.elvin", "elvin.serifov@sis.edu.az", "Elvin", "Şərifov", "ST2026015", 4, dsProg.Id);
            Console.WriteLine("--> [DbInitializer] Students seeded.");

            // 7. Kurslar (cəmi 10 kurs, kreditləri 2-5, hər birinə müəllim təyin edilib, aktiv Payız semestrindədir)
            var cIT101 = await GetOrCreateCourseAsync(context, "IT101", "Proqramlaşdırmanın Əsasları (C#)", 4, "C# dili, obyekt yönümlü proqramlaşdırma və əsas alqoritmlər.", itProg.Id, fallTerm.Id, teacherRasim.Id);
            var cIT102 = await GetOrCreateCourseAsync(context, "IT102", "Verilənlər Bazası Sistemləri (SQL)", 4, "Relyasiyalı verilənlər bazaları, SQL sorğuları və verilənlərin normallaşdırılması.", itProg.Id, fallTerm.Id, teacherLeyla.Id);
            var cIT201 = await GetOrCreateCourseAsync(context, "IT201", "Veb Proqramlaşdırma (ASP.NET Core)", 5, "Müasir veb tətbiqlər, RESTful API və MVC memarlığı.", itProg.Id, fallTerm.Id, teacherRasim.Id);
            var cCE202 = await GetOrCreateCourseAsync(context, "CE202", "Kompüter Şəbəkələri və Əməliyyat Sistemləri", 4, "OSI modeli, TCP/IP protokolları, Linux mühiti və şəbəkə inzibatçılığı.", ceProg.Id, fallTerm.Id, teacherLeyla.Id);
            var cFIN101 = await GetOrCreateCourseAsync(context, "FIN101", "Maliyyə Uçotunun Əsasları", 3, "Maliyyə hesabatları, balans, mənfəət və zərər hesabatının təhlili.", finProg.Id, fallTerm.Id, teacherMurad.Id);
            var cBBA201 = await GetOrCreateCourseAsync(context, "BBA201", "Strateji Menecment və Liderlik", 3, "Təşkilati idarəetmə, SWOT təhlili, liderlik və komanda motivasiyası.", bbaProg.Id, fallTerm.Id, teacherMurad.Id);
            var cDES101 = await GetOrCreateCourseAsync(context, "DES101", "Qrafik Dizayn və Rəqəmsal Kompozisiya", 3, "Vizual kompozisiya, rəng nəzəriyyəsi, tipoqrafika və vektor qrafikası.", desProg.Id, fallTerm.Id, teacherGunay.Id);
            var cUIX201 = await GetOrCreateCourseAsync(context, "UIX201", "İstifadəçi İnterfeysi (UI/UX) Dizaynı", 4, "İstifadəçi tədqiqatı, wireframing, interaktiv prototip və dizayn sistemləri.", uiuxProg.Id, fallTerm.Id, teacherGunay.Id);
            var cMTH101 = await GetOrCreateCourseAsync(context, "MTH101", "Ali Riyaziyyat və Xətti Cəbr", 4, "Matrislər, determinantlar, xətti tənliklər və vektor fəzaları.", amProg.Id, fallTerm.Id, teacherKamran.Id);
            var cDS201 = await GetOrCreateCourseAsync(context, "DS201", "Məlumat Analitikası və Maşın Öyrənməsi", 5, "Python ilə məlumat təhlili, vizuallaşdırma və ilkin maşın öyrənməsi modelləri.", dsProg.Id, fallTerm.Id, teacherKamran.Id);
            Console.WriteLine("--> [DbInitializer] Courses seeded.");

            // 8. Qeydiyyatlar (Enrollments - hər tələbə 3-4 kursa yazılır)
            var enrollmentsToEnsure = new List<(User Student, Course Course)>
            {
                // Aydan (4 kurs)
                (sAydan, cIT101), (sAydan, cIT102), (sAydan, cIT201), (sAydan, cUIX201),
                // Elmir (4 kurs)
                (sElmir, cIT101), (sElmir, cIT102), (sElmir, cIT201), (sElmir, cCE202),
                // Nicat (3 kurs)
                (sNicat, cIT101), (sNicat, cIT102), (sNicat, cMTH101),
                // Nigar (4 kurs)
                (sNigar, cIT101), (sNigar, cCE202), (sNigar, cMTH101), (sNigar, cDS201),
                // Kənan (4 kurs)
                (sKenan, cIT102), (sKenan, cIT201), (sKenan, cCE202), (sKenan, cDS201),
                // Leyla B. (3 kurs)
                (sLeyla, cFIN101), (sLeyla, cBBA201), (sLeyla, cMTH101),
                // Orxan (4 kurs)
                (sOrxan, cFIN101), (sOrxan, cBBA201), (sOrxan, cIT102), (sOrxan, cMTH101),
                // Sevinc (4 kurs)
                (sSevinc, cFIN101), (sSevinc, cBBA201), (sSevinc, cDES101), (sSevinc, cUIX201),
                // Tural (3 kurs)
                (sTural, cBBA201), (sTural, cFIN101), (sTural, cIT101),
                // Günel (3 kurs)
                (sGunel, cDES101), (sGunel, cUIX201), (sGunel, cIT101),
                // Kamran C. (4 kurs)
                (sKamran, cDES101), (sKamran, cUIX201), (sKamran, cIT201), (sKamran, cCE202),
                // Fidan (4 kurs)
                (sFidan, cDES101), (sFidan, cUIX201), (sFidan, cIT201), (sFidan, cDS201),
                // Samir (3 kurs)
                (sSamir, cMTH101), (sSamir, cDS201), (sSamir, cIT101),
                // Aygün (4 kurs)
                (sAygun, cMTH101), (sAygun, cDS201), (sAygun, cIT102), (sAygun, cIT201),
                // Elvin (4 kurs)
                (sElvin, cDS201), (sElvin, cMTH101), (sElvin, cIT201), (sElvin, cCE202)
            };

            var enrollmentEntities = new List<Enrollment>();
            foreach (var (st, cr) in enrollmentsToEnsure)
            {
                var enr = await context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == st.Id && e.CourseId == cr.Id);
                if (enr == null)
                {
                    enr = new Enrollment
                    {
                        StudentId = st.Id,
                        CourseId = cr.Id,
                        EnrollmentDate = DateTime.UtcNow.AddMonths(-2),
                        Status = "Enrolled"
                    };
                    await context.Enrollments.AddAsync(enr);
                    await context.SaveChangesAsync();
                }
                enrollmentEntities.Add(enr);
            }
            Console.WriteLine("--> [DbInitializer] Enrollments seeded.");

            // 9. Dərs Sessiyaları və Davamiyyət (Hər kurs üçün 4 dərs sessiyası və tələbələrin davamiyyəti)
            var allCourses = new[] { cIT101, cIT102, cIT201, cCE202, cFIN101, cBBA201, cDES101, cUIX201, cMTH101, cDS201 };

            var courseSessionTopics = new Dictionary<string, string[]>
            {
                ["IT101"] = new[] { "C# Sintaksisi, Dəyişənlər və Tiplər", "Şərt Operatorları və Dövrlər", "Obyektyönlü Proqramlaşdırma (OOP)", "Kolleksiyalar və LINQ Sorğuları" },
                ["IT102"] = new[] { "Relyasiyalı Modellər və Cədvəl Arxitekturası", "SQL Əsasları: SELECT, INSERT, UPDATE, DELETE", "JOIN Əməliyyatları və Aqreqat Funksiyalar", "Tranzaksiyalar və İndeksləşdirmə" },
                ["IT201"] = new[] { "ASP.NET Core Memarlığı və Dependency Injection", "RESTful API Dizaynı və Controller Strukturu", "Entity Framework Core və Verilənlər Bazası", "JWT Təhlükəsizlik və Avtorizasiya" },
                ["CE202"] = new[] { "Kompüter Şəbəkələri və OSI Modeli", "TCP/IP Protokollar Yığını və Marşrutlaşdırma", "Linux Əməliyyat Sistemi və Proseslər", "Şəbəkə Təhlükəsizliyi və Firewall" },
                ["FIN101"] = new[] { "Mühasibat Uçotuna Giriş və Hesablar Planı", "Maliyyə Hesabatları: Balans və Mənfəət", "Aktivlər və Öhdəliklərin Qiymətləndirilməsi", "Pul Vəsaitlərinin Hərəkəti və Audit" },
                ["BBA201"] = new[] { "Müasir Menecmentin Əsasları və Strukturlar", "Strateji Planlaşdırma və SWOT Analizi", "Liderlik Tərzləri və Komandanın Motivasiyası", "Risklərin İdarə Edilməsi və Qərar Qəbuletmə" },
                ["DES101"] = new[] { "Qrafik Dizaynın Əsasları: Xətt və Forma", "Tipoqrafika Qaydaları və Kompozisiya", "Vektor Qrafikası və İllüstrasiya", "Rəqəmsal Brendinq və Vizual Kimlik" },
                ["UIX201"] = new[] { "İstifadəçi Təcrübəsi (UX) Tədqiqatı və Persona", "İnformasiya Arxitekturası və Wireframe", "UI Komponentləri və Dizayn Sistemləri", "İnteraktiv Prototip və İstifadəçi Testləri" },
                ["MTH101"] = new[] { "Xətti Tənliklər Sistemləri və Matrislər", "Determinantlar və Tərs Matrislər", "Vektor Fəzaları və Xətti Çevirmələr", "Funksiyanın Limiti və Törəmənin Tətbiqləri" },
                ["DS201"] = new[] { "Məlumat Elminə Giriş və Python (NumPy/Pandas)", "Verilənlərin Təmizlənməsi və EDA Analizi", "Məlumatların Vizuallaşdırılması (Seaborn)", "Reqressiya və Təsnifat Modelləri" }
            };

            var sessionDates = new[]
            {
                new DateTime(2026, 10, 5),
                new DateTime(2026, 10, 12),
                new DateTime(2026, 10, 19),
                new DateTime(2026, 10, 26)
            };

            var attendancesToAdd = new List<Attendance>();

            foreach (var course in allCourses)
            {
                var enrolledStudentIds = enrollmentEntities
                    .Where(e => e.CourseId == course.Id)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .ToList();

                var topics = courseSessionTopics[course.Code];

                for (int sIndex = 0; sIndex < 4; sIndex++)
                {
                    var lessonDate = sessionDates[sIndex].AddDays(course.Id % 5);
                    var topic = topics[sIndex];

                    foreach (var studentId in enrolledStudentIds)
                    {
                        var exists = await context.Attendances.AnyAsync(a =>
                            a.CourseId == course.Id &&
                            a.StudentId == studentId &&
                            a.LessonDate.Date == lessonDate.Date);

                        if (!exists)
                        {
                            // Qarışıq statuslar: təxminən 75% Gəldi, 15% Qayıb, 10% İcazəli
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

            // 10. Qiymətlər (Müxtəlif bal və hərf qiymətləri ilə, 45-100 aralığında, GPA fərqliliyi təmin edilir)
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
                { ("student.nicat", "MTH101"), (29m, 19m, 46m, "Xətti cəbr və matris hesablamalarını dərindən mənimsəyib.") },

                // student.nigar
                { ("student.nigar", "IT101"), (24m, 17m, 42m, "C# sintaksisi və metodların qurulmasını yaxşı bilir.") },
                { ("student.nigar", "CE202"), (22m, 16m, 38m, "Kompüter şəbəkələri üzrə nəzəriyyəni yaxşı qavrayıb.") },
                { ("student.nigar", "MTH101"), (26m, 18m, 44m, "Ali riyaziyyat üzrə tapşırıqları müntəzəm yerinə yetirir.") },
                { ("student.nigar", "DS201"), (28m, 18m, 45m, "Python və verilənlərin təmizlənməsi layihəsini əla tamamladı.") },

                // student.kenan
                { ("student.kenan", "IT102"), (21m, 15m, 36m, "SQL sorğularında bəzi səhvlər olsa da, ümumi bilikləri kafidir.") },
                { ("student.kenan", "IT201"), (19m, 14m, 32m, "Veb tətbiqlərdə daha çox praktika tələb olunur.") },
                { ("student.kenan", "CE202"), (24m, 16m, 41m, "Şəbəkə protokollarını yaxşı mənimsəyib.") },
                { ("student.kenan", "DS201"), (22m, 15m, 40m, "Məlumatların vizuallaşdırılması üzrə qrafikləri düzgün qurdu.") },

                // student.leyla (Əlaçı tələbə)
                { ("student.leyla", "FIN101"), (29m, 19m, 46m, "Maliyyə hesabatlarının təhlili üzrə yüksək səviyyəli bilik nümayiş etdirdi.") },
                { ("student.leyla", "BBA201"), (27m, 18m, 44m, "Strateji idarəetmə və biznes keyslərinin təhlilində çox fəal idi.") },
                { ("student.leyla", "MTH101"), (28m, 18m, 46m, "Ali riyaziyyat imtahanından yüksək nəticə göstərdi.") },

                // student.orxan (Orta-aşağı nəticə)
                { ("student.orxan", "FIN101"), (18m, 13m, 32m, "Mühasibat uçotunun əsas anlayışlarını təkrar etmək tövsiyə olunur.") },
                { ("student.orxan", "BBA201"), (21m, 15m, 38m, "Menecment dərslərində iştirakı qənaətbəxşdir.") },
                { ("student.orxan", "IT102"), (17m, 12m, 29m, "Verilənlər bazası üzrə praktiki tapşırıqlarda çətinlik çəkir.") },
                { ("student.orxan", "MTH101"), (20m, 14m, 34m, "Riyaziyyat üzrə kafi nəticə göstərdi.") },

                // student.sevinc
                { ("student.sevinc", "FIN101"), (26m, 18m, 44m, "Maliyyə uçotunda praktiki məsələləri sərbəst həll etdi.") },
                { ("student.sevinc", "BBA201"), (28m, 18m, 46m, "Liderlik və komanda idarəçiliyi təqdimatını əla təqdim etdi.") },
                { ("student.sevinc", "DES101"), (25m, 18m, 42m, "Qrafik kompozisiyası və rəng ahəngdarlığı zövqlüdür.") },
                { ("student.sevinc", "UIX201"), (27m, 18m, 45m, "İstifadəçi tədqiqatı hesabatını dəqiqliklə hazırladı.") },

                // student.tural
                { ("student.tural", "BBA201"), (16m, 11m, 28m, "Semestr boyu dərslərə davamiyyətini artırması vacibdir.") },
                { ("student.tural", "FIN101"), (19m, 13m, 30m, "Maliyyə terminlərini mənimsəyib, amma tapşırıqlarda gecikmələri var.") },
                { ("student.tural", "IT101"), (20m, 15m, 36m, "Proqramlaşdırmanın təməl anlayışlarını qavrayıb.") },

                // student.gunel
                { ("student.gunel", "DES101"), (29m, 19m, 47m, "Qrafik dizayn üzrə portfolio işləri yüksək peşəkarlıqla yerinə yetirilib.") },
                { ("student.gunel", "UIX201"), (28m, 19m, 45m, "İnterfeys komponentləri və istifadəçi axınlarını səliqəli qurdu.") },
                { ("student.gunel", "IT101"), (27m, 18m, 44m, "Veb sahəsində kodlaşdırma biliklərini uğurla nümayiş etdirdi.") },

                // student.kamran
                { ("student.kamran", "DES101"), (23m, 16m, 39m, "Vektor illüstrasiyaları üzərində daha çox məşq etməsi lazımdır.") },
                { ("student.kamran", "UIX201"), (25m, 17m, 40m, "Dizayn sistemi prinsiplərini düzgün tətbiq etdi.") },
                { ("student.kamran", "IT201"), (22m, 15m, 37m, "ASP.NET Core üzrə layihəsini müvəffəqiyyətlə qorudu.") },
                { ("student.kamran", "CE202"), (20m, 14m, 34m, "Şəbəkə topologiyaları üzrə biliyi kafidir.") },

                // student.fidan
                { ("student.fidan", "DES101"), (27m, 19m, 45m, "Brendinq və loqotip dizaynı layihəsini əla hazırladı.") },
                { ("student.fidan", "UIX201"), (26m, 17m, 43m, "Mobil tətbiq interfeysi üzrə yaxşı prototip qurdu.") },
                { ("student.fidan", "IT201"), (28m, 19m, 46m, "Frontend və backend inteqrasiyasını mükəmməl qurdu.") },
                { ("student.fidan", "DS201"), (26m, 18m, 44m, "Məlumatların vizual qrafik təqdimatını yüksək səviyyədə etdi.") },

                // student.samir (Kəsr balı olan tələbə - F)
                { ("student.samir", "MTH101"), (13m, 9m, 26m, "Yekun bal keçid həddini (51 bal) aşmadı. Təkrar imtahan tələb olunur.") },
                { ("student.samir", "DS201"), (19m, 14m, 32m, "Python əsaslarını bilir, amma maşın öyrənməsi tapşırıqlarında çətinlik çəkdi.") },
                { ("student.samir", "IT101"), (21m, 15m, 36m, "C# dili üzrə kafi nəticə topladı.") },

                // student.aygun (Maksimum nəticəli əlaçı tələbə - A)
                { ("student.aygun", "MTH101"), (30m, 20m, 48m, "Riyaziyyat və xətti cəbr üzrə kursun ən fəal və savadlı tələbəsidir.") },
                { ("student.aygun", "DS201"), (29m, 19m, 47m, "Maşın öyrənməsi alqoritmlərini və Python kitabxanalarını dərindən bilir.") },
                { ("student.aygun", "IT102"), (28m, 19m, 45m, "Verilənlər bazası layihəsini yüksək dəqiqliklə tamamladı.") },
                { ("student.aygun", "IT201"), (29m, 19m, 48m, "REST API və mikroservis arxitekturalı layihəni mükəmməl qurdu.") },

                // student.elvin
                { ("student.elvin", "DS201"), (24m, 17m, 41m, "Məlumat analitikası layihəsini yaxşı icra etdi.") },
                { ("student.elvin", "MTH101"), (23m, 16m, 40m, "Riyazi modelləşdirmə üzrə yaxşı biliklərə malikdir.") },
                { ("student.elvin", "IT201"), (25m, 18m, 42m, "Veb layihəsini tam şəkildə təhvil verdi.") },
                { ("student.elvin", "CE202"), (28m, 18m, 45m, "Əməliyyat sistemləri daxili arxitekturasını dərindən mənimsəyib.") }
            };

            foreach (var enr in enrollmentEntities)
            {
                var student = await context.Users.FindAsync(enr.StudentId);
                var course = await context.Courses.FindAsync(enr.CourseId);

                if (student != null && course != null)
                {
                    var existingGrade = await context.Grades.FirstOrDefaultAsync(g => g.EnrollmentId == enr.Id);
                    if (existingGrade == null)
                    {
                        var key = (student.Username, course.Code);
                        if (gradesDataset.TryGetValue(key, out var gradeData))
                        {
                            var total = gradeData.Midterm + gradeData.Assignment + gradeData.Final;
                            var (letter, point) = GetLetterAndPoint(total);

                            var newGrade = new Grade
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
                            };
                            await context.Grades.AddAsync(newGrade);
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("--> [DbInitializer] Grades seeded.");

            // 11. Elanlar (Cəmi 4 elan: 2 müəssisə üzrə ümumi, 2 kursa aid fərdi)
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
                    "Hörmətli tələbələr, IT101 kursu üzrə 1-ci və 2-ci laboratoriya tapşırıqları portalda yerləşdirilmişdir. Hər bir tələbə tapşırıqları göstərilən vaxtadək təhvil verməlidir.",
                    teacherRasim.Id,
                    cIT101.Id
                ),
                (
                    "Aralıq Qiymətləndirmə (Kollokvium) Qaydaları",
                    "Diqqət! FIN101 kursu üzrə 1-ci kollokvium imtahanı noyabr ayının 20-də keçiriləcəkdir. İmtahan mövzuları ilk 4 mühazirəni əhatə edir. Hər kəsə uğurlar!",
                    teacherMurad.Id,
                    cFIN101.Id
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

        #region Helper Metodlar (İdempotent Yaratma)

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
                await context.SaveChangesAsync();
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
                await context.SaveChangesAsync();
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
                await context.SaveChangesAsync();
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
                await context.SaveChangesAsync();
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
                await context.SaveChangesAsync();
            }
            else
            {
                // Əgər kurs mövcuddursa, amma müəllim təyin olunmayıbsa və ya krediti yenilənməlidirsə
                bool changed = false;
                if (course.TeacherId == null || course.TeacherId != teacherId)
                {
                    course.TeacherId = teacherId;
                    changed = true;
                }
                if (course.Credits != credits)
                {
                    course.Credits = credits;
                    changed = true;
                }
                if (changed)
                {
                    await context.SaveChangesAsync();
                }
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
