using System;
using System.Collections.Generic;

namespace PRN232.LMS.Services.Models.SubjectModels;

public class SubjectResponseModel
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public int Credit { get; set; }
}