using FlightBookingSystem.API.Middleware;
using FlightBookingSystem.API.Profiles;
using FlightBookingSystem.API.Security;
using FlightBookingSystem.Core.Interfaces.Security;
using FlightBookingSystem.Core.Interfaces.Services;
using FlightBookingSystem.Data;
using FlightBookingSystem.Data.Seed;
using FlightBookingSystem.Services.Security;
using FlightBookingSystem.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using System.Text;
using System.Text.Json.Serialization;

const string ClientAppCorsPolicy = "ClientApp";

var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // --- Logging: route everything through NLog ---
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // --- CORS: allow the React (Vite) dev server to call this API directly ---
    builder.Services.AddCors(options =>
        options.AddPolicy(ClientAppCorsPolicy, policy =>
            policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()));

    // --- MVC / JSON ---
    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddSwaggerGen(ConfigureSwagger);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();
    builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

    // --- Application layers ---
    builder.Services.AddDataLayer(builder.Configuration);

    // --- Authentication / Authorization ---
    var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
              ?? throw new InvalidOperationException("The 'Jwt' configuration section is missing.");

    // The signing key is a secret: it lives in User Secrets / environment variables, never in appsettings.json.
    if (string.IsNullOrWhiteSpace(jwt.SecretKey) || jwt.SecretKey.Length < 32)
        throw new InvalidOperationException(
            "'Jwt:SecretKey' is missing or shorter than 32 characters. Set it with: " +
            "dotnet user-secrets set \"Jwt:SecretKey\" \"<long random value>\" --project FlightBookingSystem.API " +
            "(or the environment variable Jwt__SecretKey outside Development).");

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

    builder.Services.AddAuthorization();
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IFlightService, FlightService>();
    builder.Services.AddScoped<ISeatService, SeatService>();
    builder.Services.AddScoped<IBookingService, BookingService>();
    builder.Services.AddScoped<IAircraftService, AircraftService>();

    var app = builder.Build();

    // --- Pipeline ---
    // Global error boundary first, then correlation id, so every later log line is correlated.
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // In development the React client reaches the API through the Vite proxy over plain HTTP.
    // Redirecting those calls to HTTPS sends the browser to a different origin, and browsers drop the
    // Authorization header on cross-origin redirects - so every authenticated call came back 401.
    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    app.UseCors(ClientAppCorsPolicy);

    // Authentication must run before Authorization.
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    await SeedDatabaseAsync(app);

    app.Run();
}
catch (HostAbortedException)
{
    // Raised by the EF Core design-time tooling (migrations / database update) after it has built the
    // host to read configuration. Not an error - let the tool continue.
}
catch (Exception ex)
{
    logger.Error(ex, "Application terminated unexpectedly during startup.");
    throw;
}
finally
{
    LogManager.Shutdown();
}

static void ConfigureSwagger(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FlightBookingSystem API",
        Version = "v1",
        Description = "Web API for searching flights, viewing seats and booking them under optimistic concurrency."
    });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT returned by /api/auth/login.",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });

    var xmlDoc = Path.Combine(AppContext.BaseDirectory, $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlDoc))
        options.IncludeXmlComments(xmlDoc);
}

static async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(context, passwordHasher, app.Configuration["Seed:AdminPassword"]);
}

/// <summary>Exposed so integration tests can reference the composition root.</summary>
public partial class Program;
