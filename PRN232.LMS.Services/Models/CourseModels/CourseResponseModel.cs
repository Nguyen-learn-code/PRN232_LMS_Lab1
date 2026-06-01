using System;
using System.Collections.Generic;

namespace PRN232.LMS.Services.Models.CourseModels;

public class CourseResponseModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
    public string? SemesterName { get; set; }
    
    // Complete related data
    //public List<CourseEnrollmentModel> Enrollments { get; set; } = new();
}

public class CourseEnrollmentModel
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? Status { get; set; }
    public DateTime? EnrollDate { get; set; }
}