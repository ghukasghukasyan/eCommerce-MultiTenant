using eCommerce.Application.DTOs.Identity;
using FluentValidation;

namespace eCommerce.Application.Validations.Authentication
{
    public class LoginUserValidator : AbstractValidator<LoginUserDTO>
    {
        public LoginUserValidator()
        {
            RuleFor(user => user.Email)
             .NotEmpty().WithMessage("Email is required.")
             .EmailAddress().WithMessage("A valid email is required.");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
