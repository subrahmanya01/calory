using Calory.Ai.Orchestrator.Handlers;
using Calory.Ai.Orchestrator.Options;
using Microsoft.Agents.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection("AzureOpenAI"));
builder.Services.Configure<McpOptions>(
    builder.Configuration.GetSection(McpOptions.SectionName));

builder.Services.AddSingleton<CaloryAgentRuntime>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var options = configuration
        .GetSection("AzureOpenAI")
        .Get<AzureOpenAIOptions>()
        ?? throw new InvalidOperationException(
            "AzureOpenAI configuration is missing.");

    var mcpOptions = sp.GetRequiredService<IConfiguration>()
        .GetSection(McpOptions.SectionName)
        .Get<McpOptions>()
        ?? throw new InvalidOperationException("Mcp configuration is missing.");

    return CaloryAgentRuntime.CreateAsync(options, mcpOptions)
        .GetAwaiter()
        .GetResult();
});
builder.Services.AddSingleton<AIAgent>(sp => sp.GetRequiredService<CaloryAgentRuntime>().Agent);
builder.Services.AddScoped<IAiOrchestrator, AiOrchestrator>();

builder.Services.AddControllers();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();