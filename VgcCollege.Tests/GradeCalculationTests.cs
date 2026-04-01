namespace VgcCollege.Tests;

public class GradeCalculationTests
{
    [Theory]
    [InlineData(90, "A")]
    [InlineData(80, "B")]
    [InlineData(70, "C")]
    [InlineData(60, "D")]
    [InlineData(50, "F")]
    public void CalculateGrade_ReturnsCorrectGrade(int score, string expectedGrade)
    {
        int maxScore = 100;
        string grade = CalculateGrade(score, maxScore);
        Assert.Equal(expectedGrade, grade);
    }

    private string CalculateGrade(int score, int maxScore)
    {
        double percentage = (double)score / maxScore * 100;
        
        if (percentage >= 90) return "A";
        if (percentage >= 80) return "B";
        if (percentage >= 70) return "C";
        if (percentage >= 60) return "D";
        return "F";
    }
}