using Microsoft.AspNetCore.Identity;

namespace VgcCollege.Web.Models;

public class FacultyProfile
{
    public int Id { get; set; }
    public string IdentityUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    
    public IdentityUser? IdentityUser { get; set; }
    public ICollection<Course> TaughtCourses { get; set; } = new List<Course>();
}