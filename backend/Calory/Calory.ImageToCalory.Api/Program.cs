using Calory.ImageToCalory.Api.AiModelStrategy;
using Calory.ImageToCalory.Api.Clients;
using Calory.ImageToCalory.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<NvidiaClient>(client =>
{
    client.BaseAddress =
        new Uri("https://integrate.api.nvidia.com/v1/");
});

builder.Services.AddScoped<IAiModelStrategy, KimiK3Strategy>();
builder.Services.AddScoped<IAiModelStrategy, NemotronStrategy>();
builder.Services.AddScoped<IPromptService, PromptService>();


builder.Services.AddScoped<
    IAiModelStrategyResolver,
    AiModelStrategyResolver>();

builder.Services.AddScoped<
    IAiService,
    AiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "AI API V1");

        options.RoutePrefix = "swagger";
    });
}

app.MapControllers();

app.Run();