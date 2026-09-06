using Calory.Api.Features.Users;
using Calory.Domain;
using Calory.Persistance.Interfaces;
using Microsoft.AspNetCore.Identity;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;

namespace Calory.Api.Mcp;

[McpServerToolType]
public sealed class UserTools(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IHttpContextAccessor httpContextAccessor)
{
    [McpServerTool, Description("Gets the authenticated user's profile.")]
    public async Task<string> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(GetUserId(), cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new KeyNotFoundException("The authenticated user was not found.");
        }

        return JsonSerializer.Serialize(UserResponse.From(user));
    }
    
    private Guid GetUserId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : throw new UnauthorizedAccessException("A valid authenticated user is required.");
    }
}