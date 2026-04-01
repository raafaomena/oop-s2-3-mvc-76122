using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<StudentProfile> StudentProfiles { get; set; }
    public DbSet<FacultyProfile> FacultyProfiles { get; set; }
    public DbSet<CourseEnrolment> CourseEnrolments { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<AssignmentResult> AssignmentResults { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<ExamResult> ExamResults { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Course>()
            .HasOne(c => c.FacultyProfile)
            .WithMany(f => f.TaughtCourses)
            .HasForeignKey(c => c.FacultyProfileId)
            .OnDelete(DeleteBehavior.SetNull);
            
        modelBuilder.Entity<CourseEnrolment>()
            .HasOne(ce => ce.StudentProfile)
            .WithMany(s => s.Enrolments)
            .HasForeignKey(ce => ce.StudentProfileId);
            
        modelBuilder.Entity<CourseEnrolment>()
            .HasOne(ce => ce.Course)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(ce => ce.CourseId);
            
        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(ar => ar.CourseEnrolment)
            .WithMany(ce => ce.AttendanceRecords)
            .HasForeignKey(ar => ar.CourseEnrolmentId);
            
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Course)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.CourseId);
            
        modelBuilder.Entity<AssignmentResult>()
            .HasOne(ar => ar.Assignment)
            .WithMany(a => a.Results)
            .HasForeignKey(ar => ar.AssignmentId);
            
        modelBuilder.Entity<AssignmentResult>()
            .HasOne(ar => ar.StudentProfile)
            .WithMany(s => s.AssignmentResults)
            .HasForeignKey(ar => ar.StudentProfileId);
            
        modelBuilder.Entity<Exam>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Exams)
            .HasForeignKey(e => e.CourseId);
            
        modelBuilder.Entity<ExamResult>()
            .HasOne(er => er.Exam)
            .WithMany(e => e.Results)
            .HasForeignKey(er => er.ExamId);
            
        modelBuilder.Entity<ExamResult>()
            .HasOne(er => er.StudentProfile)
            .WithMany(s => s.ExamResults)
            .HasForeignKey(er => er.StudentProfileId);
    }
}