using System;
using FluentValidation;
using PRN232.LMS.Services.Models.StudentModels;

namespace PRN232.LMS.Services.Validators;

// FluentValidation validator
public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required via FluentValidation.")
            .Length(3, 100).WithMessage("Full Name length must be between 3 and 100.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("A valid email is required via FluentValidation.");

        RuleFor(x => x.StudentCode)
            .NotEmpty()
            .Must(BeValidFptuCode).WithMessage("Student Code must be in FPTU Style (e.g. SE19886) via FluentValidation.");
    }

    private bool BeValidFptuCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(code, @"^(SE|CE|IA|HE|GD|MC)\d{5,6}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
