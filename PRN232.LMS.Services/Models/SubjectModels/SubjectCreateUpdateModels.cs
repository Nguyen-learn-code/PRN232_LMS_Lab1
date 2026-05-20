namespace PRN232.LMS.Services.Models.SubjectModels;

public class SubjectCreateModel
{
    public string SubjectCode { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public int Credit { get; set; }
}

public class SubjectUpdateModel
{
    public string SubjectCode { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public int Credit { get; set; }
}