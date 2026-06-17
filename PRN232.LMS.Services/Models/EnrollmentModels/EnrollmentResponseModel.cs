using System;
using PRN232.LMS.Services.Models.StudentModels;
using PRN232.LMS.Services.Models.CourseModels;

namespace PRN232.LMS.Services.Models.EnrollmentModels;

public class EnrollmentResponseModel
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;

    public StudentResponseModel? Student { get; set; }
    public CourseResponseModel? Course { get; set; }
}
