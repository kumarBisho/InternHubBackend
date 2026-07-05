using InternMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using InternMS.Api.DTOs;
using InternMS.Api.DTOs.Authentication;
using InternMS.Api.DTOs.Users;
using InternMS.Api.Services;
using InternMS.Api.Services.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InternMS.Api.Hubs;
using InternMS.Api.Services.Auth;
using InternMS.Api.Services.Token;
using InternMS.Api.Services.Projects;
using InternMS.Api.Services.Users;
using InternMS.Api.Middleware;
using InternMS.Api.Services.Email;
using InternMS.Api.Services.Dashboard;
using InternMS.Api.Services.Analytics;
using InternMS.Api.Services.Search;
using InternMS.Api.Services.Collaboration;
using InternMS.Api.Services.Feedback;
using Microsoft.AspNetCore.SignalR;
using FluentValidation;
using InternMS.Api.Validators;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Core;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// Serilog Configuration - Structured Logging
// ============================================================================
builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "InternMS.Api")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/internms-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            fileSizeLimitBytes: 1024 * 1024 * 100 // 100 MB
        );

    if (context.HostingEnvironment.IsDevelopment())
    {
        loggerConfig.MinimumLevel.Debug();
    }
});

// Determine frontend URL based on environment
var isDevelopment = builder.Environment.IsDevelopment();
var frontendUrl = isDevelopment 
    ? builder.Configuration["Frontend:DevUrl"] ?? "http://localhost:5173"
    : builder.Configuration["Frontend:ProdUrl"] ?? "https://internhub.com";

// Controllers + Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Application Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ICollaborationService, CollaborationService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

// FluentValidation
builder.Services.AddScoped<IValidator<CreateUserDto>, CreateUserDtoValidator>();
builder.Services.AddScoped<IValidator<LoginRequestDto>, LoginRequestDtoValidator>();
builder.Services.AddScoped<IValidator<RefreshTokenRequestDto>, RefreshTokenRequestDtoValidator>();

// SignalR with authentication
builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(10);
})
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
});

// Configure SignalR UserIdProvider for proper user routing
builder.Services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// PostgreSQL + EF Core
// builder.Services.AddDbContextPool<AppDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
// );

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("ConnectionStrings__DefaultConnection is missing.");
}

Console.WriteLine($"Connection string begins with: {connectionString[..20]}");

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        b => b.MigrationsAssembly("InternMS.Infrastructure")
    ));

// JWT Settings
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (jwtKey is null)
{
    throw new Exception("JWT Key is missing in configuration.");
}

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Enforce HTTPS in production for security
    options.RequireHttpsMetadata = !isDevelopment;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
    
    // For SignalR: Accept token from query string as well
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            
            // If the request is for a SignalR hub and there's no Authorization header,
            // try to get the token from the query string
            if (string.IsNullOrEmpty(context.Token) && !string.IsNullOrEmpty(accessToken) &&
                context.HttpContext.WebSockets.IsWebSocketRequest)
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

// CORS
// builder.Services.AddCors(options =>
// {
//     if (isDevelopment)
//     {
//         // Development: Allow multiple common dev ports
//         options.AddPolicy("AllowFrontend",
//             p => p
//                 .WithOrigins("http://localhost:5173", "https://internhubfrontend-yvjd.onrender.com/", frontendUrl)
//                 .AllowAnyHeader()
//                 .AllowAnyMethod()
//                 .AllowCredentials()
//         );
//     }
//     else
//     {
//         // Production: Only allow specific frontend URL
//         options.AddPolicy("AllowFrontend",
//             p => p
//                 .WithOrigins(frontendUrl)
//                 .AllowAnyHeader()
//                 .AllowAnyMethod()
//                 .AllowCredentials()
//         );
//     }
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://internhubfrontend-yvjd.onrender.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var MigrationLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        MigrationLogger.LogWarning("===== MIGRATION START =====");

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Database.Migrate();

        MigrationLogger.LogWarning("===== MIGRATION SUCCESS =====");
    }
    catch (Exception ex)
    {
        MigrationLogger.LogError(ex, "Migration failed");
        throw;
    }
}

// ============================================================================
// Middleware Pipeline - OPTIMAL ORDER FOR SECURITY AND PERFORMANCE
// ============================================================================
// Order matters! Each middleware must be in the correct position for proper
// request/response handling and security

// 1. Exception Handling (MUST be first - catches all errors)
app.UseGlobalExceptionHandler();

// 2. Swagger/API Documentation (development only)
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseSwagger();
app.UseSwaggerUI();

// 3. Security Headers (protect against common attacks)
app.UseSecurityHeaders();

// 4. Rate Limiting (protect against brute force and DDoS)
app.UseRateLimiting();

// 5. Request/Response Logging (for debugging and monitoring)
app.UseRequestLogging();

// 6. CORS Policy (allow/deny cross-origin requests)
app.UseCors("AllowFrontend");

// 7. HTTPS Redirection (production only - redirect HTTP to HTTPS)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 8. Authentication (verify JWT tokens)
app.UseAuthentication();

// 9. Custom Middleware (token blacklist - prevent logout bypass)
app.UseMiddleware<TokenBlacklistMiddleware>();

// 10. Authorization (check user roles and permissions)
app.UseAuthorization();

// 11. Routing and Endpoints
app.MapControllers();

// SignalR Hubs
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<CollaborationHub>("/hubs/collaboration");

// Log startup information with port details
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("================================");
logger.LogInformation("🚀 InternMS API Starting Up");
logger.LogInformation("================================");
if (app.Urls.Any())
{
    foreach (var url in app.Urls)
    {
        logger.LogInformation("📍 Server Running on: {Url}", url);
    }
}
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("================================");

app.Run();
