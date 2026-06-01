using System;
using System.Collections.Generic;

namespace PRN232.LMS.Services.Models.StudentModels;

public class StudentResponseModel
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    
    // Complete related data
    //public List<StudentEnrollmentModel> Enrollments { get; set; } = new();
}

public class StudentEnrollmentModel
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? Status { get; set; }
    public DateTime? EnrollDate { get; set; }
}