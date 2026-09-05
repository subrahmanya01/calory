using FastEndpoints;
using FluentValidation;

namespace Calory.Api.Features.Auth.Login;

public sealed class LoginRequestValidator : Validator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Invalid email address.");
        RuleFor(request => request.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}