namespace VgcCollege.Web.Models;

public class CourseEnrolment
{
    public int Id { get; set; }
    public int StudentProfileId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolDate { get; set; }
    public string Status { get; set; } = "Active";
    
    public StudentProfile? StudentProfile { get; set; }
    public Course? Course { get; set; }
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}