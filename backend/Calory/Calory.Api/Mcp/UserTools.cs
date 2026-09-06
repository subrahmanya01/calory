using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using Calory.Api.Features.Users;
using Calory.Api.Features.Users.UpdateUser;
using Calory.Domain;
using Calory.Persistance.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ModelContextProtocol.Server;

namespace Calory.Api.Mcp;

[McpServerToolType]
public sealed class UserTools(
    IUnitOfWork unitOfWork,
    IPasswordHasher<User> passwordHasher,
    IHttpContextAccessor httpContextAccessor)
{
    [McpServerTool, Description("Gets the authenticated user's profile.")]
    public async Task<string> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(GetUserId(), cancellationToken);
        if (user is null || !user.IsActive)
            throw new KeyNotFoundException("The authenticated user was not found.");

        return JsonSerializer.Serialize(UserResponse.From(user));
    }

    [McpServerTool, Description("Updates the authenticated user's name, email, and optional password.")]
    public async Task<string> UpdateCurrentUser(
        [Description("The replacement profile values. Password is optional and is never returned.")] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(GetUserId(), cancellationToken);
        if (user is null || !user.IsActive)
            throw new KeyNotFoundException("The authenticated user was not found.");

        var email = request.Email.Trim().ToLowerInvariant();
        var existingUser = await unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (existingUser is not null && existingUser.Id != user.Id)
            throw new InvalidOperationException("That email address is already in use.");

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        user.LastUpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return JsonSerializer.Serialize(UserResponse.From(user));
    }

    [McpServerTool, Description("Deactivates the authenticated user's account.")]
    public async Task<string> DeleteCurrentUser(CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(GetUserId(), cancellationToken);
        if (user is null)
            throw new KeyNotFoundException("The authenticated user was not found.");

        user.IsActive = false;
        user.LastUpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return JsonSerializer.Serialize(new { Deleted = true, Id = user.Id });
    }

    private Guid GetUserId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("A valid authenticated user is required.");
    }
}