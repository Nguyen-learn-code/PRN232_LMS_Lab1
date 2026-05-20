using System;

namespace PRN232.LMS.Services.Models.StudentModels;

public class StudentCreateModel
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}

public class StudentUpdateModel
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}