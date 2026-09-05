using Calory.Api.Features.Users;

namespace Calory.Api.Features.Auth.Login;

public sealed record LoginResponse(string Token, UserResponse User);