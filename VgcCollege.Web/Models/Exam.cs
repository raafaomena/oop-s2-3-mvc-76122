namespace VgcCollege.Web.Models;

public class Exam
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int MaxScore { get; set; }
    public bool ResultsReleased { get; set; }
    
    public Course? Course { get; set; }
    public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
}