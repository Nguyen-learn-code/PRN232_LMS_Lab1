using System;

namespace PRN232.LMS.Services.Models.SemesterModels;

public class SemesterCreateModel
{
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SemesterUpdateModel
{
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}