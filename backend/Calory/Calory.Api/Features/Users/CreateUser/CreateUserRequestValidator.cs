using FastEndpoints;
using FluentValidation;

namespace Calory.Api.Features.Users.CreateUser
{
    public class CreateUserRequestValidator : Validator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("First name is required and must be at most 100 characters long.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Last name is required and must be at most 100 characters long.");

            RuleFor(x => x.Email)
                .NotEmpty().NotNull()
                .EmailAddress()
                .WithMessage("Email is required and must be a valid email address.");
            RuleFor(x => x.Password)
                .NotEmpty().NotNull()
                .MinimumLength(8)
                .WithMessage("Password is required and must be at least 8 characters long.");
        }
    }
}
