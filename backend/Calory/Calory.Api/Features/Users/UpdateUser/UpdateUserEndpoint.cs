using Calory.Domain;
using Calory.Persistance.Repository;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Calory.Api.Features.Users.UpdateUser;

public sealed class UpdateUserEndpoint(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher) : Endpoint<UpdateUserRequest, UserResponse>
{
    public override void Configure()
    {
        Put("/api/users/me");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var existingUser = await unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (existingUser is not null && existingUser.Id != user.Id)
        {
            await Send.StatusCodeAsync(409, cancellationToken);
            return;
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        user.LastUpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await Send.OkAsync(UserResponse.From(user), cancellationToken);
    }
}