using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

public class StudentProfileController : Controller
{
    private readonly ApplicationDbContext _context;

    public StudentProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        
        List<StudentProfile> students;

        if (isAdmin)
        {
            students = await _context.StudentProfiles.ToListAsync();
        }
        else
        {
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
            
            if (faculty == null)
            {
                return Forbid();
            }
            
            var facultyCourseIds = await _context.Courses
                .Where(c => c.FacultyProfileId == faculty.Id)
                .Select(c => c.Id)
                .ToListAsync();
            
            var studentIds = await _context.CourseEnrolments
                .Where(e => facultyCourseIds.Contains(e.CourseId))
                .Select(e => e.StudentProfileId)
                .Distinct()
                .ToListAsync();
            
            students = await _context.StudentProfiles
                .Where(s => studentIds.Contains(s.Id))
                .ToListAsync();
        }

        return View(students);
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var student = await _context.StudentProfiles
            .FirstOrDefaultAsync(m => m.Id == id);
        if (student == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        
        if (!isAdmin)
        {
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
            
            if (faculty != null)
            {
                var facultyCourseIds = await _context.Courses
                    .Where(c => c.FacultyProfileId == faculty.Id)
                    .Select(c => c.Id)
                    .ToListAsync();
                
                var isStudentInFacultyCourse = await _context.CourseEnrolments
                    .AnyAsync(e => e.StudentProfileId == student.Id && facultyCourseIds.Contains(e.CourseId));
                
                if (!isStudentInFacultyCourse)
                {
                    return Forbid();
                }
            }
        }

        return View(student);
    }

    [Authorize(Roles = "Admin,Faculty")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Create(StudentProfile student)
    {
        if (ModelState.IsValid)
        {
            _context.Add(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(student);
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var student = await _context.StudentProfiles.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }
        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Edit(int id, StudentProfile student)
    {
        if (id != student.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(student.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(student);
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var student = await _context.StudentProfiles
            .FirstOrDefaultAsync(m => m.Id == id);
        if (student == null)
        {
            return NotFound();
        }

        return View(student);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await _context.StudentProfiles.FindAsync(id);
        if (student != null)
        {
            _context.StudentProfiles.Remove(student);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool StudentExists(int id)
    {
        return _context.StudentProfiles.Any(e => e.Id == id);
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var student = await _context.StudentProfiles
            .FirstOrDefaultAsync(s => s.IdentityUserId == userId);

        if (student == null)
        {
            return NotFound("Student profile not found. Please contact support.");
        }

        var enrolments = await _context.CourseEnrolments
            .Include(e => e.Course)
            .Where(e => e.StudentProfileId == student.Id)
            .ToListAsync();

        var assignmentResults = await _context.AssignmentResults
            .Include(r => r.Assignment)
            .ThenInclude(a => a.Course)
            .Where(r => r.StudentProfileId == student.Id)
            .ToListAsync();

        ViewBag.Enrolments = enrolments;
        ViewBag.AssignmentResults = assignmentResults;
        
        return View(student);
    }
}