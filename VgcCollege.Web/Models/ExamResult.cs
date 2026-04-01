namespace VgcCollege.Web.Models;

public class ExamResult
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public int StudentProfileId { get; set; }
    public int Score { get; set; }
    public string Grade { get; set; } = string.Empty;
    
    public Exam? Exam { get; set; }
    public StudentProfile? StudentProfile { get; set; }
}