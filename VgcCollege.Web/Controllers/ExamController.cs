using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize]
public class ExamController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExamController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Index(int? courseId)
    {
        var query = _context.Exams
            .Include(e => e.Course)
            .ThenInclude(c => c.Branch)
            .AsQueryable();

        if (courseId.HasValue)
        {
            query = query.Where(e => e.CourseId == courseId.Value);
            ViewBag.CourseId = courseId.Value;
            var course = await _context.Courses.FindAsync(courseId);
            ViewBag.CourseName = course?.Name;
        }

        ViewBag.Courses = await _context.Courses.ToListAsync();
        var exams = await query.ToListAsync();
        return View(exams);
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Results(int? examId)
    {
        if (examId == null)
        {
            return NotFound();
        }

        var exam = await _context.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == examId);

        if (exam == null)
        {
            return NotFound();
        }

        var students = await _context.CourseEnrolments
            .Where(e => e.CourseId == exam.CourseId && e.Status == "Active")
            .Include(e => e.StudentProfile)
            .Select(e => e.StudentProfile)
            .ToListAsync();

        var results = await _context.ExamResults
            .Where(r => r.ExamId == examId)
            .ToDictionaryAsync(r => r.StudentProfileId);

        ViewBag.Exam = exam;
        ViewBag.Results = results;
        ViewBag.Students = students;

        return View();
    }

    [Authorize(Roles = "Admin,Faculty")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(int examId, int studentId, int score, string grade)
    {
        var existing = await _context.ExamResults
            .FirstOrDefaultAsync(r => r.ExamId == examId && r.StudentProfileId == studentId);

        if (existing != null)
        {
            existing.Score = score;
            existing.Grade = grade;
            _context.Update(existing);
        }
        else
        {
            var result = new ExamResult
            {
                ExamId = examId,
                StudentProfileId = studentId,
                Score = score,
                Grade = grade
            };
            _context.Add(result);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Results), new { examId });
    }

    [Authorize(Roles = "Admin,Faculty")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRelease(int examId)
    {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam != null)
        {
            exam.ResultsReleased = !exam.ResultsReleased;
            _context.Update(exam);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyResults()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var student = await _context.StudentProfiles
            .FirstOrDefaultAsync(s => s.IdentityUserId == userId);

        if (student == null)
        {
            return NotFound("Student profile not found");
        }

        var examResults = await _context.ExamResults
            .Include(r => r.Exam)
            .ThenInclude(e => e.Course)
            .Where(r => r.StudentProfileId == student.Id)
            .ToListAsync();

        var releasedResults = examResults
            .Where(r => r.Exam.ResultsReleased)
            .ToList();

        var provisionalResults = examResults
            .Where(r => !r.Exam.ResultsReleased)
            .ToList();

        ViewBag.ReleasedResults = releasedResults;
        ViewBag.ProvisionalResults = provisionalResults;
        ViewBag.StudentName = student.Name;

        return View();
    }

    [Authorize(Roles = "Admin,Faculty")]
    public IActionResult Create()
    {
        ViewBag.Courses = _context.Courses.ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Exam exam)
    {
        if (ModelState.IsValid)
        {
            exam.ResultsReleased = false;
            _context.Add(exam);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Courses = _context.Courses.ToList();
        return View(exam);
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var exam = await _context.Exams.FindAsync(id);
        if (exam == null)
        {
            return NotFound();
        }
        ViewBag.Courses = _context.Courses.ToList();
        return View(exam);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Exam exam)
    {
        if (id != exam.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(exam);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamExists(exam.Id))
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
        return View(exam);
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var exam = await _context.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (exam == null)
        {
            return NotFound();
        }

        return View(exam);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam != null)
        {
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool ExamExists(int id)
    {
        return _context.Exams.Any(e => e.Id == id);
    }
}