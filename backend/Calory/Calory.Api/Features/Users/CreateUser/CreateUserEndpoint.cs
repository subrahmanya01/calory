using Calory.Domain;
using Calory.Persistance.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Calory.Api.Features.Users.CreateUser
{
    public sealed class CreateUserEndpoint(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher) : Endpoint<CreateUserRequest, UserResponse>
    {
        public override void Configure()
        {
            Post("/api/users");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await unitOfWork.Users.GetByEmailAsync(email, cancellationToken) is not null)
            {
                await Send.StatusCodeAsync(409, cancellationToken);
                return;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            unitOfWork.Users.Add(user);

            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                await Send.StatusCodeAsync(409, cancellationToken);
                return;
            }

            await Send.ResponseAsync(UserResponse.From(user), 201, cancellationToken);
        }
    }
}
