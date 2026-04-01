using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin,Faculty")]
public class CourseEnrolmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public CourseEnrolmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var enrolments = await _context.CourseEnrolments
            .Include(c => c.StudentProfile)
            .Include(c => c.Course)
            .ToListAsync();
        return View(enrolments);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var enrolment = await _context.CourseEnrolments
            .Include(c => c.StudentProfile)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (enrolment == null)
        {
            return NotFound();
        }

        return View(enrolment);
    }

    public IActionResult Create()
    {
        ViewBag.Students = _context.StudentProfiles.ToList();
        ViewBag.Courses = _context.Courses.ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseEnrolment enrolment)
    {
        if (ModelState.IsValid)
        {
            enrolment.EnrolDate = DateTime.Today;
            enrolment.Status = "Active";
            _context.Add(enrolment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Students = _context.StudentProfiles.ToList();
        ViewBag.Courses = _context.Courses.ToList();
        return View(enrolment);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var enrolment = await _context.CourseEnrolments.FindAsync(id);
        if (enrolment == null)
        {
            return NotFound();
        }
        ViewBag.Students = _context.StudentProfiles.ToList();
        ViewBag.Courses = _context.Courses.ToList();
        return View(enrolment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseEnrolment enrolment)
    {
        if (id != enrolment.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(enrolment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrolmentExists(enrolment.Id))
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
        ViewBag.Students = _context.StudentProfiles.ToList();
        ViewBag.Courses = _context.Courses.ToList();
        return View(enrolment);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var enrolment = await _context.CourseEnrolments
            .Include(c => c.StudentProfile)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (enrolment == null)
        {
            return NotFound();
        }

        return View(enrolment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var enrolment = await _context.CourseEnrolments.FindAsync(id);
        if (enrolment != null)
        {
            _context.CourseEnrolments.Remove(enrolment);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool EnrolmentExists(int id)
    {
        return _context.CourseEnrolments.Any(e => e.Id == id);
    }
}