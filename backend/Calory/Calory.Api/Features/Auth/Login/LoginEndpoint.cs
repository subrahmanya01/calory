using Calory.Api.Features.Users;
using Calory.Api.Infrastructure;
using Calory.Domain;
using Calory.Persistance.Repository;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;

namespace Calory.Api.Features.Auth.Login;

public sealed class LoginEndpoint(
    IUnitOfWork unitOfWork,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenService tokenService) : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsActive || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            await Send.StatusCodeAsync(401, cancellationToken);
            return;
        }

        await Send.OkAsync(new LoginResponse(tokenService.CreateToken(user), UserResponse.From(user)), cancellationToken);
    }
}