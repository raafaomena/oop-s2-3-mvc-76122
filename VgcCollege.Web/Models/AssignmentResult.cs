namespace VgcCollege.Web.Models;

public class AssignmentResult
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public int StudentProfileId { get; set; }
    public int Score { get; set; }
    public string? Feedback { get; set; }
    
    public Assignment? Assignment { get; set; }
    public StudentProfile? StudentProfile { get; set; }
}