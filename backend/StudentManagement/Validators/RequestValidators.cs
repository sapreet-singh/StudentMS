using FluentValidation;
using StudentManagement.DTOs.Requests;

namespace StudentManagement.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(100)
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers and underscores.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        RuleFor(x => x.RoleName).Must(r => new[] { "Admin", "Teacher", "Student" }.Contains(r))
            .WithMessage("Role must be Admin, Teacher, or Student.");
    }
}

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20)
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Invalid phone number format.");
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today.AddYears(-5)).WithMessage("Date of birth seems invalid.")
            .GreaterThan(DateTime.Today.AddYears(-100)).WithMessage("Date of birth seems too old.");
        RuleFor(x => x.EnrollmentDate).NotEmpty().LessThanOrEqualTo(DateTime.Today.AddDays(1));
        RuleFor(x => x.CourseId).GreaterThan(0).WithMessage("Please select a valid course.");
    }
}

public class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20)
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Invalid phone number format.");
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.EnrollmentDate).NotEmpty();
        RuleFor(x => x.CourseId).GreaterThan(0);
    }
}
