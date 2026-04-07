using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Create Roles
        string[] roles = { "Admin", "Faculty", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create Admin User
        string adminEmail = "admin@vgc.com";
        IdentityUser adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Create Faculty User
        string facultyEmail = "faculty@vgc.com";
        IdentityUser facultyUser = await userManager.FindByEmailAsync(facultyEmail);
        if (facultyUser == null)
        {
            facultyUser = new IdentityUser
            {
                UserName = facultyEmail,
                Email = facultyEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(facultyUser, "Faculty@123");
            await userManager.AddToRoleAsync(facultyUser, "Faculty");
        }

        // Create Student Users
        string[] studentEmails = { "alice@vgc.com", "bob@vgc.com", "carol@vgc.com", "david@vgc.com", "emma@vgc.com" };
        string[] studentNames = { "Alice Johnson", "Bob Williams", "Carol Davis", "David Miller", "Emma Wilson" };
        string[] studentNumbers = { "STU001", "STU002", "STU003", "STU004", "STU005" };
        string[] studentAddresses = { "10 College Road, Dublin", "15 Park Avenue, Cork", "8 Bay View, Galway", "22 Church Street, Dublin", "5 Harbour View, Cork" };
        DateTime[] studentDOBs = { new DateTime(2000, 5, 15), new DateTime(2001, 8, 20), new DateTime(1999, 3, 10), new DateTime(2000, 11, 25), new DateTime(2002, 1, 30) };

        var studentUsers = new List<IdentityUser>();
        for (int i = 0; i < studentEmails.Length; i++)
        {
            var studentUser = await userManager.FindByEmailAsync(studentEmails[i]);
            if (studentUser == null)
            {
                studentUser = new IdentityUser
                {
                    UserName = studentEmails[i],
                    Email = studentEmails[i],
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(studentUser, "Student@123");
                await userManager.AddToRoleAsync(studentUser, "Student");
            }
            studentUsers.Add(studentUser);
        }

        // Create Branches
        if (!context.Branches.Any())
        {
            context.Branches.AddRange(
                new Branch { Name = "Dublin Branch", Address = "1 Main Street, Dublin 1" },
                new Branch { Name = "Cork Branch", Address = "2 Main Street, Cork" },
                new Branch { Name = "Galway Branch", Address = "3 Main Street, Galway" }
            );
            await context.SaveChangesAsync();
        }

        // Create Faculty Profile
        if (!context.FacultyProfiles.Any())
        {
            context.FacultyProfiles.Add(new FacultyProfile
            {
                IdentityUserId = facultyUser.Id,
                Name = "Professor John Smith",
                Email = facultyEmail,
                Phone = "01 123 4568"
            });
            await context.SaveChangesAsync();
        }

        // Create Student Profiles
        var facultyProfile = await context.FacultyProfiles.FirstOrDefaultAsync();
        
        for (int i = 0; i < studentUsers.Count; i++)
        {
            if (!context.StudentProfiles.Any(s => s.IdentityUserId == studentUsers[i].Id))
            {
                context.StudentProfiles.Add(new StudentProfile
                {
                    IdentityUserId = studentUsers[i].Id,
                    Name = studentNames[i],
                    Email = studentEmails[i],
                    Phone = $"087 {1234567 + i}",
                    Address = studentAddresses[i],
                    DateOfBirth = studentDOBs[i],
                    StudentNumber = studentNumbers[i]
                });
            }
        }
        await context.SaveChangesAsync();

        // Create Courses
        var dublinBranch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Dublin Branch");
        var corkBranch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Cork Branch");
        var galwayBranch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Galway Branch");

        if (!context.Courses.Any())
        {
            context.Courses.AddRange(
                new Course { Name = "Computer Science", BranchId = dublinBranch.Id, FacultyProfileId = facultyProfile?.Id, StartDate = new DateTime(2026, 9, 1), EndDate = new DateTime(2027, 5, 31) },
                new Course { Name = "Business Management", BranchId = dublinBranch.Id, FacultyProfileId = facultyProfile?.Id, StartDate = new DateTime(2026, 9, 1), EndDate = new DateTime(2027, 5, 31) },
                new Course { Name = "Software Engineering", BranchId = corkBranch.Id, FacultyProfileId = facultyProfile?.Id, StartDate = new DateTime(2026, 9, 1), EndDate = new DateTime(2027, 5, 31) },
                new Course { Name = "Data Science", BranchId = galwayBranch.Id, FacultyProfileId = facultyProfile?.Id, StartDate = new DateTime(2026, 9, 1), EndDate = new DateTime(2027, 5, 31) }
            );
            await context.SaveChangesAsync();
        }

        // Create Enrolments
        if (!context.CourseEnrolments.Any())
        {
            var students = await context.StudentProfiles.ToListAsync();
            var courses = await context.Courses.ToListAsync();
            var enrolments = new List<CourseEnrolment>();
            var rnd = new Random();

            foreach (var student in students)
            {
                int numCourses = rnd.Next(1, 3);
                var shuffledCourses = courses.OrderBy(x => rnd.Next()).Take(numCourses);

                foreach (var course in shuffledCourses)
                {
                    enrolments.Add(new CourseEnrolment
                    {
                        StudentProfileId = student.Id,
                        CourseId = course.Id,
                        EnrolDate = DateTime.Today.AddDays(-rnd.Next(0, 30)),
                        Status = "Active"
                    });
                }
            }

            if (enrolments.Any())
            {
                context.CourseEnrolments.AddRange(enrolments);
                await context.SaveChangesAsync();
            }
        }

        // Create Assignments
        if (!context.Assignments.Any())
        {
            var courses = await context.Courses.ToListAsync();
            var assignments = new List<Assignment>();

            foreach (var course in courses)
            {
                assignments.Add(new Assignment { CourseId = course.Id, Title = "Assignment 1 - Introduction", MaxScore = 100, DueDate = DateTime.Today.AddDays(14) });
                assignments.Add(new Assignment { CourseId = course.Id, Title = "Assignment 2 - Midterm Project", MaxScore = 100, DueDate = DateTime.Today.AddDays(30) });
            }

            context.Assignments.AddRange(assignments);
            await context.SaveChangesAsync();
        }

        // Create Assignment Results
        if (!context.AssignmentResults.Any())
        {
            var assignments = await context.Assignments.ToListAsync();
            var enrolments = await context.CourseEnrolments.ToListAsync();
            var results = new List<AssignmentResult>();
            var rnd = new Random();

            foreach (var assignment in assignments)
            {
                var studentsInCourse = enrolments.Where(e => e.CourseId == assignment.CourseId).Select(e => e.StudentProfileId).Distinct();

                foreach (var studentId in studentsInCourse)
                {
                    results.Add(new AssignmentResult
                    {
                        AssignmentId = assignment.Id,
                        StudentProfileId = studentId,
                        Score = rnd.Next(65, 98),
                        Feedback = "Good work! Keep it up."
                    });
                }
            }

            if (results.Any())
            {
                context.AssignmentResults.AddRange(results);
                await context.SaveChangesAsync();
            }
        }

        // Create Exams
        if (!context.Exams.Any())
        {
            var courses = await context.Courses.ToListAsync();
            var exams = new List<Exam>();

            foreach (var course in courses)
            {
                exams.Add(new Exam { CourseId = course.Id, Title = "Midterm Exam", Date = DateTime.Today.AddDays(21), MaxScore = 100, ResultsReleased = false });
                exams.Add(new Exam { CourseId = course.Id, Title = "Final Exam", Date = DateTime.Today.AddDays(60), MaxScore = 100, ResultsReleased = false });
            }

            context.Exams.AddRange(exams);
            await context.SaveChangesAsync();
        }

        // Create Exam Results
        if (!context.ExamResults.Any())
        {
            var exams = await context.Exams.ToListAsync();
            var enrolments = await context.CourseEnrolments.ToListAsync();
            var results = new List<ExamResult>();
            var rnd = new Random();

            foreach (var exam in exams)
            {
                var studentsInCourse = enrolments.Where(e => e.CourseId == exam.CourseId).Select(e => e.StudentProfileId).Distinct();

                foreach (var studentId in studentsInCourse)
                {
                    int score = rnd.Next(55, 95);
                    string grade = score >= 70 ? "A" : score >= 60 ? "B" : score >= 50 ? "C" : "D";
                    results.Add(new ExamResult { ExamId = exam.Id, StudentProfileId = studentId, Score = score, Grade = grade });
                }
            }

            if (results.Any())
            {
                context.ExamResults.AddRange(results);
                await context.SaveChangesAsync();
            }
        }

        // Create Attendance Records
        if (!context.AttendanceRecords.Any())
        {
            var enrolments = await context.CourseEnrolments.ToListAsync();
            var records = new List<AttendanceRecord>();
            var rnd = new Random();

            foreach (var enrolment in enrolments)
            {
                for (int week = 1; week <= 8; week++)
                {
                    records.Add(new AttendanceRecord
                    {
                        CourseEnrolmentId = enrolment.Id,
                        WeekNumber = week,
                        Date = DateTime.Today.AddDays(-(8 - week) * 7),
                        Present = rnd.Next(0, 100) > 20
                    });
                }
            }

            if (records.Any())
            {
                context.AttendanceRecords.AddRange(records);
                await context.SaveChangesAsync();
            }
        }
    }
}