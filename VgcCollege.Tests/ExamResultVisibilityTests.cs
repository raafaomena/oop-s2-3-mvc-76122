using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Tests;

public class ExamResultVisibilityTests
{
    [Fact]
    public async Task StudentCannotSeeProvisionalResults()
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

        var exam = new Exam
        {
            Title = "Test Exam",
            CourseId = course.Id,
            Date = DateTime.Today,
            MaxScore = 100,
            ResultsReleased = false
        };
        
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var result = new ExamResult
        {
            ExamId = exam.Id,
            StudentProfileId = student.Id,
            Score = 85,
            Grade = "B"
        };
        
        context.ExamResults.Add(result);
        await context.SaveChangesAsync();

        var visibleResults = await context.ExamResults
            .Include(r => r.Exam)
            .Where(r => r.StudentProfileId == student.Id && r.Exam.ResultsReleased)
            .ToListAsync();
        
        Assert.Empty(visibleResults);
    }

    [Fact]
    public async Task StudentCanSeeReleasedResults()
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

        var exam = new Exam
        {
            Title = "Test Exam",
            CourseId = course.Id,
            Date = DateTime.Today,
            MaxScore = 100,
            ResultsReleased = true
        };
        
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var result = new ExamResult
        {
            ExamId = exam.Id,
            StudentProfileId = student.Id,
            Score = 92,
            Grade = "A"
        };
        
        context.ExamResults.Add(result);
        await context.SaveChangesAsync();

        var visibleResults = await context.ExamResults
            .Include(r => r.Exam)
            .Where(r => r.StudentProfileId == student.Id && r.Exam.ResultsReleased)
            .ToListAsync();
        
        Assert.Single(visibleResults);
        Assert.Equal(92, visibleResults.First().Score);
    }
}