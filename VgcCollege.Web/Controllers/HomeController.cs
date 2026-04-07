using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return View("Welcome");
        }

        if (User.IsInRole("Admin"))
        {
            var totalStudents = await _context.StudentProfiles.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var totalEnrolments = await _context.CourseEnrolments.CountAsync();
            var totalFaculty = await _context.FacultyProfiles.CountAsync();
            
            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalEnrolments = totalEnrolments;
            ViewBag.TotalFaculty = totalFaculty;
            
            return View("AdminDashboard");
        }
        else if (User.IsInRole("Faculty"))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
            
            if (faculty != null)
            {
                var myCourses = await _context.Courses
                    .Where(c => c.FacultyProfileId == faculty.Id)
                    .Include(c => c.Enrolments)
                    .ToListAsync();
                
                var totalStudents = myCourses.Sum(c => c.Enrolments.Count);
                var totalAssignments = await _context.Assignments
                    .CountAsync(a => myCourses.Select(c => c.Id).Contains(a.CourseId));
                
                ViewBag.MyCourses = myCourses;
                ViewBag.TotalCourses = myCourses.Count;
                ViewBag.TotalStudents = totalStudents;
                ViewBag.TotalAssignments = totalAssignments;
                ViewBag.FacultyName = faculty.Name;
            }
            
            return View("FacultyDashboard");
        }
        else if (User.IsInRole("Student"))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.IdentityUserId == userId);
            
            if (student != null)
            {
                var myEnrolments = await _context.CourseEnrolments
                    .Where(e => e.StudentProfileId == student.Id)
                    .Include(e => e.Course)
                    .ToListAsync();
                
                var myAssignments = await _context.AssignmentResults
                    .Where(r => r.StudentProfileId == student.Id)
                    .Include(r => r.Assignment)
                    .ToListAsync();
                
                ViewBag.MyEnrolments = myEnrolments;
                ViewBag.MyAssignments = myAssignments;
                ViewBag.TotalCourses = myEnrolments.Count;
                ViewBag.TotalAssignments = myAssignments.Count;
                ViewBag.StudentName = student.Name;
            }
            
            return View("StudentDashboard");
        }
        
        return View("Welcome");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}