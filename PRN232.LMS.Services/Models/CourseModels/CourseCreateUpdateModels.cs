namespace PRN232.LMS.Services.Models.CourseModels;

public class CourseCreateModel
{
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
}

public class CourseUpdateModel
{
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
}