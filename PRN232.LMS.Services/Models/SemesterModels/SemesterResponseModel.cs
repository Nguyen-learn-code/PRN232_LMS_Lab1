using System;
using System.Collections.Generic;

namespace PRN232.LMS.Services.Models.SemesterModels;

public class SemesterResponseModel
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = null!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Related data
    public List<SemesterCourseModel> Courses { get; set; } = new();
}

public class SemesterCourseModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
}