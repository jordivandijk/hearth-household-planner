using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using HouseholdPlanner.Api.Authentication;
using HouseholdPlanner.Api.Data;
using HouseholdPlanner.Api.Models;
using HouseholdPlanner.Api.Repositories.Implementations;
using HouseholdPlanner.Api.Repositories.Interfaces;
using HouseholdPlanner.Api.Services;
using HouseholdPlanner.Api.Services.Implementations;
using HouseholdPlanner.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Description = "Paste your JWT token or an API key here (without the 'Bearer ' prefix)."
            }
        };
        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, ct) =>
    {
        if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer")] = []
                }
            ];
        }
        return Task.CompletedTask;
    });
});

// HS256 needs a key of at least 256 bits; without one every sign-in fails with a 500.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes. Generate one with: openssl rand -base64 32");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing: the API needs a PostgreSQL database.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// A request carries either a sign-in token from the app or an API key from an automation, both as a Bearer token.
// API keys start with "hearth_", which a JWT never does.
const string jwtOrApiKey = "JwtOrApiKey";
builder.Services.AddAuthentication(jwtOrApiKey)
    .AddPolicyScheme(jwtOrApiKey, jwtOrApiKey, options =>
        options.ForwardDefaultSelector = context => ApiKeyAuthenticationHandler.KeyFrom(context.Request) is null
            ? JwtBearerDefaults.AuthenticationScheme
            : ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, null)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };

        // A token is valid for weeks, longer than the account it was issued for may stay the same.
        // Reject it once the user is deleted, their role or household changes, or their password changes on another
        // device (the web app then asks them to sign in again), and use their current username, so a rename shows up
        // without signing in again.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var user = Guid.TryParse(identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)
                    ? await db.Users.AsNoTracking()
                        .Where(u => u.Id == userId)
                        .Select(u => new { u.Username, u.Role, u.HouseholdId, u.SecurityStamp })
                        .FirstOrDefaultAsync(context.HttpContext.RequestAborted)
                    : null;

                if (identity is null || user is null
                    || !identity.HasClaim(identity.RoleClaimType, user.Role.ToString())
                    || !identity.HasClaim(TokenService.HouseholdClaim, user.HouseholdId.ToString())
                    || !identity.HasClaim(TokenService.SecurityStampClaim, user.SecurityStamp.ToString()))
                {
                    context.Fail("The account was deleted, or its role, household or password changed.");
                    return;
                }

                if (identity.Name != user.Username && identity.FindFirst(identity.NameClaimType) is { } name)
                {
                    identity.RemoveClaim(name);
                    identity.AddClaim(new Claim(identity.NameClaimType, user.Username));
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHouseholdRepository, HouseholdRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IGroceryRepository, GroceryRepository>();
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHouseholdService, HouseholdService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IGroceryService, GroceryService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<SignInLimiter>();

var corsOrigins = builder.Configuration["CorsOrigins"]
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["http://localhost:5224"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbSeeder));
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db, config, logger);
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
// Error messages follow the app's language, which it sends as Accept-Language.
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(Languages.Default)
    .AddSupportedCultures([.. Languages.Supported])
    .AddSupportedUICultures([.. Languages.Supported]));
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
