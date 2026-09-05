using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Calory.Api.Features.Users.CreateUser
{
    public sealed class CreateUserEndpoint : Endpoint<CreateUserRequest, string>
    {
        public CreateUserEndpoint()
        {
            
        }

        public override void Configure()
        {
            Post("/api/user");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateUserRequest request, CancellationToken cancellationToken)
        {
            await Send.OkAsync("User created successfully.", cancellationToken);
        }
    }
}
