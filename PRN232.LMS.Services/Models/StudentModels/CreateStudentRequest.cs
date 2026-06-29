using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace PRN232.LMS.Services.Models.StudentModels;

// Custom validation attribute for FPTU Style Student Code
public class FptuCodeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // handled by [Required]
        }

        var code = value.ToString() ?? "";
        
        // FPTU Student Code pattern: e.g., SE19886, CE18793
        if (System.Text.RegularExpressions.Regex.IsMatch(code, @"^(SE|CE|IA|HE|GD|MC)\d{5,6}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("Student Code must be in FPTU Style (e.g., SE19886, CE18793).");
    }
}

public class CreateStudentRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters.")]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = null!;

    [Required]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string Phone { get; set; } = null!;

    [Required]
    [Range(18, 100, ErrorMessage = "Age must be between 18 and 100.")]
    public int Age { get; set; }

    [Required]
    [FptuCode] // Custom Validation Attribute
    [RegularExpression(@"^(SE|CE|IA|HE|GD|MC)\d{5,6}$", ErrorMessage = "Student Code must match regex pattern.")]
    public string StudentCode { get; set; } = null!;

    [Required]
    public DateTime DateOfBirth { get; set; }
}

public class UpdateStudentRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    public string Phone { get; set; } = null!;

    [Required]
    [Range(18, 100)]
    public int Age { get; set; }

    [Required]
    [FptuCode]
    public string StudentCode { get; set; } = null!;

    [Required]
    public DateTime DateOfBirth { get; set; }
}



public class StudentQueryRequest : PRN232.LMS.Repositories.Models.QueryModels.QueryParameters
{
}
