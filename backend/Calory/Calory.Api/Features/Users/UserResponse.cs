using Calory.Domain;

namespace Calory.Api.Features.Users;

public sealed record UserResponse(Guid Id, string FirstName, string LastName, string Email)
{
    public static UserResponse From(User user) => new(user.Id, user.FirstName, user.LastName, user.Email);
}