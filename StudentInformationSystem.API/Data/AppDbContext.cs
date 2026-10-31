using Microsoft.EntityFrameworkCore;
using StudentInformationSystem.API.Entities;

namespace StudentInformationSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Programme> Programmes => Set<Programme>();
        public DbSet<Term> Terms => Set<Term>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<Grade> Grades => Set<Grade>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<Announcement> Announcements => Set<Announcement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                entity.HasOne(u => u.Programme)
                      .WithMany(p => p.Students)
                      .HasForeignKey(u => u.ProgrammeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Department)
                      .WithMany(d => d.Teachers)
                      .HasForeignKey(u => u.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasOne(c => c.Teacher)
                      .WithMany(u => u.TaughtCourses)
                      .HasForeignKey(c => c.TeacherId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(c => c.Programme)
                      .WithMany(p => p.Courses)
                      .HasForeignKey(c => c.ProgrammeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Term)
                      .WithMany(t => t.Courses)
                      .HasForeignKey(c => c.TermId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Enrollment
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();

                entity.HasOne(e => e.Student)
                      .WithMany(u => u.Enrollments)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Enrollments)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Grade
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.Property(g => g.MidtermScore).HasPrecision(5, 2);
                entity.Property(g => g.FinalScore).HasPrecision(5, 2);
                entity.Property(g => g.TotalScore).HasPrecision(5, 2);
                entity.Property(g => g.GradePoint).HasPrecision(4, 2);

                entity.HasOne(g => g.Enrollment)
                      .WithOne(e => e.Grade)
                      .HasForeignKey<Grade>(g => g.EnrollmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Attendance
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasOne(a => a.Course)
                      .WithMany(c => c.Attendances)
                      .HasForeignKey(a => a.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Student)
                      .WithMany(u => u.Attendances)
                      .HasForeignKey(a => a.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Announcement
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.HasOne(a => a.Author)
                      .WithMany(u => u.AuthoredAnnouncements)
                      .HasForeignKey(a => a.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Course)
                      .WithMany(c => c.Announcements)
                      .HasForeignKey(a => a.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
