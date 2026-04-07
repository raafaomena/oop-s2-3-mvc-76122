using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

public class AssignmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public AssignmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Index(int? courseId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        
        IQueryable<Assignment> query = _context.Assignments
            .Include(a => a.Course)
            .ThenInclude(c => c.Branch)
            .AsQueryable();

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
                
                query = query.Where(a => facultyCourseIds.Contains(a.CourseId));
            }
        }

        if (courseId.HasValue)
        {
            query = query.Where(a => a.CourseId == courseId.Value);
            ViewBag.CourseId = courseId.Value;
            var course = await _context.Courses.FindAsync(courseId);
            ViewBag.CourseName = course?.Name;
        }

        var coursesQuery = _context.Courses.AsQueryable();
        if (!isAdmin)
        {
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
            
            if (faculty != null)
            {
                coursesQuery = coursesQuery.Where(c => c.FacultyProfileId == faculty.Id);
            }
        }
        
        ViewBag.Courses = await coursesQuery.ToListAsync();
        var assignments = await query.ToListAsync();
        return View(assignments);
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Gradebook(int? assignmentId)
    {
        if (assignmentId == null)
        {
            return NotFound();
        }

        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        
        if (!isAdmin)
        {
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
            
            if (faculty != null && assignment.Course.FacultyProfileId != faculty.Id)
            {
                return Forbid();
            }
        }

        var students = await _context.CourseEnrolments
            .Where(e => e.CourseId == assignment.CourseId && e.Status == "Active")
            .Include(e => e.StudentProfile)
            .Select(e => e.StudentProfile)
            .ToListAsync();

        var results = await _context.AssignmentResults
            .Where(r => r.AssignmentId == assignmentId)
            .ToDictionaryAsync(r => r.StudentProfileId);

        ViewBag.Assignment = assignment;
        ViewBag.Results = results;
        ViewBag.Students = students;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> SaveGrade(int assignmentId, int studentId, int score, string? feedback)
    {
        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);
        
        if (assignment == null)
        {
            return NotFound();
        }
        
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        
        if (!isAdmin)
        {
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
            
            if (faculty != null && assignment.Course.FacultyProfileId != faculty.Id)
            {
                return Forbid();
            }
        }

        var existing = await _context.AssignmentResults
            .FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.StudentProfileId == studentId);

        if (existing != null)
        {
            existing.Score = score;
            existing.Feedback = feedback;
            _context.Update(existing);
        }
        else
        {
            var result = new AssignmentResult
            {
                AssignmentId = assignmentId,
                StudentProfileId = studentId,
                Score = score,
                Feedback = feedback
            };
            _context.Add(result);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Gradebook), new { assignmentId });
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        ViewBag.Courses = _context.Courses.ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Assignment assignment)
    {
        if (ModelState.IsValid)
        {
            _context.Add(assignment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Courses = _context.Courses.ToList();
        return View(assignment);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null)
        {
            return NotFound();
        }
        ViewBag.Courses = _context.Courses.ToList();
        return View(assignment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Assignment assignment)
    {
        if (id != assignment.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(assignment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssignmentExists(assignment.Id))
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
        ViewBag.Courses = _context.Courses.ToList();
        return View(assignment);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (assignment == null)
        {
            return NotFound();
        }

        return View(assignment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment != null)
        {
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool AssignmentExists(int id)
    {
        return _context.Assignments.Any(e => e.Id == id);
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyAssignments()
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
            return NotFound("Student profile not found.");
        }

        var results = await _context.AssignmentResults
            .Include(r => r.Assignment)
            .ThenInclude(a => a.Course)
            .Where(r => r.StudentProfileId == student.Id)
            .ToListAsync();

        ViewBag.StudentName = student.Name;
        return View(results);
    }
}