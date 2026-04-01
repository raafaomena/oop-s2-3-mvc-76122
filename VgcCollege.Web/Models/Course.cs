namespace VgcCollege.Web.Models;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? FacultyProfileId { get; set; }
    
    public Branch? Branch { get; set; }
    public FacultyProfile? FacultyProfile { get; set; }
    public ICollection<CourseEnrolment> Enrolments { get; set; } = new List<CourseEnrolment>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}