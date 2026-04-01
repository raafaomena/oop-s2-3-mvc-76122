using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin,Faculty")]
public class AttendanceRecordController : Controller
{
    private readonly ApplicationDbContext _context;

    public AttendanceRecordController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? enrolmentId)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.CourseEnrolment)
            .ThenInclude(c => c.StudentProfile)
            .Include(a => a.CourseEnrolment)
            .ThenInclude(c => c.Course)
            .AsQueryable();

        if (enrolmentId.HasValue)
        {
            query = query.Where(a => a.CourseEnrolmentId == enrolmentId.Value);
            ViewBag.EnrolmentId = enrolmentId.Value;
            
            var enrolment = await _context.CourseEnrolments
                .Include(c => c.StudentProfile)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == enrolmentId);
            
            if (enrolment != null)
            {
                ViewBag.StudentName = enrolment.StudentProfile?.Name;
                ViewBag.CourseName = enrolment.Course?.Name;
            }
        }

        var attendanceRecords = await query.ToListAsync();
        
        ViewBag.Enrolments = await _context.CourseEnrolments
            .Include(c => c.StudentProfile)
            .Include(c => c.Course)
            .ToListAsync();
            
        return View(attendanceRecords);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var attendance = await _context.AttendanceRecords
            .Include(a => a.CourseEnrolment)
            .ThenInclude(c => c.StudentProfile)
            .Include(a => a.CourseEnrolment)
            .ThenInclude(c => c.Course)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (attendance == null)
        {
            return NotFound();
        }

        return View(attendance);
    }

    public IActionResult Create(int? enrolmentId)
    {
        ViewBag.EnrolmentId = enrolmentId;
        ViewBag.Enrolments = _context.CourseEnrolments
            .Include(c => c.StudentProfile)
            .Include(c => c.Course)
            .ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AttendanceRecord attendance)
    {
        if (ModelState.IsValid)
        {
            attendance.Date = DateTime.Today;
            _context.Add(attendance);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { enrolmentId = attendance.CourseEnrolmentId });
        }
        
        ViewBag.Enrolments = _context.CourseEnrolments
            .Include(c => c.StudentProfile)
            .Include(c => c.Course)
            .ToList();
        return View(attendance);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var attendance = await _context.AttendanceRecords.FindAsync(id);
        if (attendance == null)
        {
            return NotFound();
        }
        
        ViewBag.Enrolments = _context.CourseEnrolments
            .Include(c => c.StudentProfile)
            .Include(c => c.Course)
            .ToList();
        return View(attendance);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AttendanceRecord attendance)
    {
        if (id != attendance.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(attendance);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttendanceExists(attendance.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index), new { enrolmentId = attendance.CourseEnrolmentId });
        }
        
        ViewBag.Enrolments = _context.CourseEnrolments
            .Include(c => c.StudentProfile)
            .Include(c => c.Course)
            .ToList();
        return View(attendance);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var attendance = await _context.AttendanceRecords
            .Include(a => a.CourseEnrolment)
            .ThenInclude(c => c.StudentProfile)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (attendance == null)
        {
            return NotFound();
        }

        return View(attendance);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var attendance = await _context.AttendanceRecords.FindAsync(id);
        var enrolmentId = attendance?.CourseEnrolmentId;
        
        if (attendance != null)
        {
            _context.AttendanceRecords.Remove(attendance);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { enrolmentId });
    }

    private bool AttendanceExists(int id)
    {
        return _context.AttendanceRecords.Any(e => e.Id == id);
    }
}