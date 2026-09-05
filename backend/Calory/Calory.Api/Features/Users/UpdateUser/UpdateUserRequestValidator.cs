using FastEndpoints;
using FluentValidation;

namespace Calory.Api.Features.Users.UpdateUser;

public sealed class UpdateUserRequestValidator : Validator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.Password)
            .MinimumLength(8)
            .When(request => !string.IsNullOrWhiteSpace(request.Password));
    }
}