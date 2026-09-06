using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text;
using Calory.Api.Infrastructure;
using Calory.Domain;
using Calory.Persistance;
using Calory.Api.Options;
using Microsoft.AspNetCore.Http.Json;


var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt configuration is required.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key) || Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes long.");
}

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(options =>
    {
        options.EnableJWTBearerAuth = true;
    });

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();
builder.Services.AddHttpContextAccessor();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.MapMcp("/mcp").RequireAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CaloryDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await dbContext.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS "HealthGoals" (
            "Id" uuid NOT NULL,
            "UserId" uuid NOT NULL,
            "DailyCalorieTarget" numeric(10,2) NOT NULL,
            "ProteinTarget" numeric(10,2) NOT NULL,
            "CarbTarget" numeric(10,2) NOT NULL,
            "FatTarget" numeric(10,2) NOT NULL,
            "WeightTarget" numeric(10,2) NOT NULL,
            "StartDate" date NOT NULL,
            "EndDate" date NULL,
            "IsActive" boolean NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_HealthGoals" PRIMARY KEY ("Id")
        );
        CREATE INDEX IF NOT EXISTS "IX_HealthGoals_UserId_IsActive"
            ON "HealthGoals" ("UserId", "IsActive");

        CREATE TABLE IF NOT EXISTS "FoodEntries" (
            "Id" uuid NOT NULL,
            "UserId" uuid NOT NULL,
            "MealType" character varying(20) NOT NULL,
            "FoodName" character varying(200) NOT NULL,
            "Quantity" numeric NOT NULL,
            "Unit" character varying(40) NOT NULL,
            "ConsumedAt" timestamp with time zone NOT NULL,
            "Source" character varying(20) NOT NULL,
            "Notes" text NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "UpdatedAt" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_FoodEntries" PRIMARY KEY ("Id")
        );
        CREATE INDEX IF NOT EXISTS "IX_FoodEntries_UserId_ConsumedAt"
            ON "FoodEntries" ("UserId", "ConsumedAt");

        CREATE TABLE IF NOT EXISTS "FoodNutrition" (
            "Id" uuid NOT NULL,
            "FoodEntryId" uuid NOT NULL,
            "Calories" numeric(10,2) NOT NULL,
            "ProteinG" numeric(10,2) NOT NULL,
            "CarbohydratesG" numeric(10,2) NOT NULL,
            "FatG" numeric(10,2) NOT NULL,
            "FiberG" numeric NOT NULL,
            "SugarG" numeric NOT NULL,
            "SodiumMg" numeric NOT NULL,
            "CalciumMg" numeric NOT NULL,
            "IronMg" numeric NOT NULL,
            "MagnesiumMg" numeric NOT NULL,
            "PotassiumMg" numeric NOT NULL,
            "ZincMg" numeric NOT NULL,
            "VitaminAMcg" numeric NOT NULL,
            "VitaminB1Mg" numeric NOT NULL,
            "VitaminB2Mg" numeric NOT NULL,
            "VitaminB3Mg" numeric NOT NULL,
            "VitaminB6Mg" numeric NOT NULL,
            "VitaminB12Mcg" numeric NOT NULL,
            "VitaminCMg" numeric NOT NULL,
            "VitaminDMcg" numeric NOT NULL,
            "VitaminEMg" numeric NOT NULL,
            "VitaminKMcg" numeric NOT NULL,
            CONSTRAINT "PK_FoodNutrition" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_FoodNutrition_FoodEntries_FoodEntryId"
                FOREIGN KEY ("FoodEntryId") REFERENCES "FoodEntries" ("Id") ON DELETE CASCADE
        );
        """);

    // if (app.Environment.IsDevelopment())
    //     await DevelopmentDataSeeder.SeedAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

app.Run();