using Calory.Domain;

namespace Calory.Api.Services;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
