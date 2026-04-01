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

        string[] roles = { "Admin", "Faculty", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        string adminEmail = "admin@vgc.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        string facultyEmail = "faculty@vgc.com";
        if (await userManager.FindByEmailAsync(facultyEmail) == null)
        {
            var facultyUser = new IdentityUser
            {
                UserName = facultyEmail,
                Email = facultyEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(facultyUser, "Faculty@123");
            await userManager.AddToRoleAsync(facultyUser, "Faculty");
        }

        string studentEmail = "student@vgc.com";
        if (await userManager.FindByEmailAsync(studentEmail) == null)
        {
            var studentUser = new IdentityUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(studentUser, "Student@123");
            await userManager.AddToRoleAsync(studentUser, "Student");
        }

        if (!context.Branches.Any())
        {
            context.Branches.AddRange(
                new Branch { Name = "Dublin Branch", Address = "1 Main St, Dublin" },
                new Branch { Name = "Cork Branch", Address = "2 Main St, Cork" },
                new Branch { Name = "Galway Branch", Address = "3 Main St, Galway" }
            );
            await context.SaveChangesAsync();
        }

        var adminUserEntity = await userManager.FindByEmailAsync(adminEmail);
        if (adminUserEntity != null && !context.FacultyProfiles.Any(f => f.IdentityUserId == adminUserEntity.Id))
        {
            context.FacultyProfiles.Add(new FacultyProfile
            {
                IdentityUserId = adminUserEntity.Id,
                Name = "Admin User",
                Email = adminEmail,
                Phone = "01 123 4567"
            });
            await context.SaveChangesAsync();
        }

        var facultyUserEntity = await userManager.FindByEmailAsync(facultyEmail);
        if (facultyUserEntity != null && !context.FacultyProfiles.Any(f => f.IdentityUserId == facultyUserEntity.Id))
        {
            context.FacultyProfiles.Add(new FacultyProfile
            {
                IdentityUserId = facultyUserEntity.Id,
                Name = "Faculty User",
                Email = facultyEmail,
                Phone = "01 123 4568"
            });
            await context.SaveChangesAsync();
        }

        var studentUserEntity = await userManager.FindByEmailAsync(studentEmail);
        if (studentUserEntity != null && !context.StudentProfiles.Any(s => s.IdentityUserId == studentUserEntity.Id))
        {
            context.StudentProfiles.Add(new StudentProfile
            {
                IdentityUserId = studentUserEntity.Id,
                Name = "Student User",
                Email = studentEmail,
                Phone = "01 123 4569",
                Address = "Student Address",
                DateOfBirth = new DateTime(2000, 1, 1),
                StudentNumber = "STU001"
            });
            await context.SaveChangesAsync();
        }

        if (!context.Courses.Any())
        {
            var dublinBranch = context.Branches.FirstOrDefault(b => b.Name == "Dublin Branch");
            if (dublinBranch != null)
            {
                context.Courses.AddRange(
                    new Course
                    {
                        Name = "Computer Science",
                        BranchId = dublinBranch.Id,
                        StartDate = new DateTime(2026, 9, 1),
                        EndDate = new DateTime(2027, 5, 31)
                    },
                    new Course
                    {
                        Name = "Business Management",
                        BranchId = dublinBranch.Id,
                        StartDate = new DateTime(2026, 9, 1),
                        EndDate = new DateTime(2027, 5, 31)
                    }
                );
                await context.SaveChangesAsync();
            }
        }

        if (!context.CourseEnrolments.Any())
        {
            var studentProfile = context.StudentProfiles.FirstOrDefault();
            var course = context.Courses.FirstOrDefault();
            if (studentProfile != null && course != null)
            {
                context.CourseEnrolments.Add(new CourseEnrolment
                {
                    StudentProfileId = studentProfile.Id,
                    CourseId = course.Id,
                    EnrolDate = DateTime.Today,
                    Status = "Active"
                });
                await context.SaveChangesAsync();
            }
        }
    }
}