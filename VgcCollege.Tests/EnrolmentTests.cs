using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Tests;

public class EnrolmentTests
{
    [Fact]
    public async Task CanEnrolStudentInCourse()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var student = new StudentProfile 
        { 
            Name = "Test Student", 
            Email = "test@test.com",
            StudentNumber = "STU001"
        };
        var course = new Course { Name = "Test Course" };
        
        context.StudentProfiles.Add(student);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var enrolment = new CourseEnrolment
        {
            StudentProfileId = student.Id,
            CourseId = course.Id,
            EnrolDate = DateTime.Today,
            Status = "Active"
        };
        
        context.CourseEnrolments.Add(enrolment);
        await context.SaveChangesAsync();

        var savedEnrolment = await context.CourseEnrolments
            .FirstOrDefaultAsync(e => e.StudentProfileId == student.Id);
        
        Assert.NotNull(savedEnrolment);
        Assert.Equal("Active", savedEnrolment.Status);
    }

    [Fact]
    public async Task StudentCanHaveMultipleEnrolments()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var student = new StudentProfile 
        { 
            Name = "Test Student", 
            Email = "test@test.com",
            StudentNumber = "STU001"
        };
        var course1 = new Course { Name = "Course 1" };
        var course2 = new Course { Name = "Course 2" };
        
        context.StudentProfiles.Add(student);
        context.Courses.AddRange(course1, course2);
        await context.SaveChangesAsync();

        var enrolment1 = new CourseEnrolment
        {
            StudentProfileId = student.Id,
            CourseId = course1.Id,
            EnrolDate = DateTime.Today,
            Status = "Active"
        };
        var enrolment2 = new CourseEnrolment
        {
            StudentProfileId = student.Id,
            CourseId = course2.Id,
            EnrolDate = DateTime.Today,
            Status = "Active"
        };
        
        context.CourseEnrolments.AddRange(enrolment1, enrolment2);
        await context.SaveChangesAsync();

        var enrolments = await context.CourseEnrolments
            .Where(e => e.StudentProfileId == student.Id)
            .ToListAsync();
        
        Assert.Equal(2, enrolments.Count);
    }
}