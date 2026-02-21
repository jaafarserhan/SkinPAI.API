using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using SkinPAI.API.Data;
using SkinPAI.API.Middleware;
using SkinPAI.API.Repositories;
using SkinPAI.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Ensure Logs directory exists before configuring Serilog
var logsPath = Path.Combine(builder.Environment.ContentRootPath, "Logs");
if (!Directory.Exists(logsPath))
{
    Directory.CreateDirectory(logsPath);
}

// Configure Serilog with comprehensive logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "SkinPAI.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj} | User: {UserId} | RequestId: {RequestId}{NewLine}{Exception}")
    .WriteTo.File(
        path: Path.Combine(logsPath, "SkinPAI-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [User:{UserId}] [RequestId:{RequestId}] [Correlation:{CorrelationId}]{NewLine}    Message: {Message:lj}{NewLine}    {Exception}",
        fileSizeLimitBytes: 10_485_760, // 10MB
        rollOnFileSizeLimit: true)
    .WriteTo.File(
        path: Path.Combine(logsPath, "SkinPAI-Errors-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90,
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [User:{UserId}] [RequestId:{RequestId}]{NewLine}    Message: {Message:lj}{NewLine}    Exception: {Exception}{NewLine}    ==================================================={NewLine}")
    .WriteTo.File(
        path: Path.Combine(logsPath, $"SkinPAI-{DateTime.Now:yyyyMMdd-HHmmss}-startup.log"),
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("========================================");
Log.Information("SkinPAI API Starting Up");
Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
Log.Information("Log Files Location: {LogsPath}", logsPath);
Log.Information("========================================");

// Add services to the container.

// Database
builder.Services.AddDbContext<SkinPAIDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IScanService, ScanService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<IRoutineService, RoutineService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// AI Skin Analysis Service (Hugging Face - Free tier)
builder.Services.AddHttpClient<ISkinAnalysisAIService, HuggingFaceSkinAnalysisService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int>("HuggingFace:TimeoutSeconds", 60));
    client.DefaultRequestHeaders.Add("User-Agent", "SkinPAI-API/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "SkinPAI-Super-Secret-Key-For-JWT-Token-Generation-2024";

builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtSettings["Issuer"] ?? "SkinPAI.API",
        ValidAudience = jwtSettings["Audience"] ?? "SkinPAI.Client",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:3000",
            "http://localhost:3001",
            "http://127.0.0.1:5173",
            "http://127.0.0.1:3000",
            "http://127.0.0.1:3001",
            "https://skinpai.app"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
    
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SkinPAI API",
        Description = "Backend API for SkinPAI skin analysis mobile application",
        Contact = new OpenApiContact
        {
            Name = "SkinPAI Support",
            Email = "support@skinpai.app"
        }
    });

    // JWT Bearer authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Ensure Uploads directory exists
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "Uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    Directory.CreateDirectory(Path.Combine(uploadsPath, "scans"));
    Directory.CreateDirectory(Path.Combine(uploadsPath, "profiles"));
    Directory.CreateDirectory(Path.Combine(uploadsPath, "posts"));
    Directory.CreateDirectory(Path.Combine(uploadsPath, "products"));
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SkinPAI API v1");
        options.DocumentTitle = "SkinPAI API Documentation";
    });
    
    // Apply migrations automatically in development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SkinPAIDbContext>();
    dbContext.Database.Migrate();
    
    // Seed data (products, distributors, brands, community data)
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DataSeeder.SeedDataAsync(dbContext, logger);
}

// Enable CORS
app.UseCors("AllowFrontend");

// Serve static files from Uploads folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseHttpsRedirection();

// Custom request logging middleware - captures user context, request/response details
app.UseRequestLogging();

// Serilog request logging for HTTP metrics
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
        diagnosticContext.Set("UserId", httpContext.User?.FindFirst("sub")?.Value ?? "Anonymous");
    };
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint with logging
app.MapGet("/health", (ILogger<Program> logger) => 
{
    logger.LogDebug("Health check endpoint called");
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow, logsPath = logsPath });
});

// Log startup complete
Log.Information("========================================");
Log.Information("SkinPAI API Started Successfully!");
Log.Information("Swagger UI: http://localhost:{{port}}/swagger");
Log.Information("Health Check: http://localhost:{{port}}/health");
Log.Information("Log Files: {LogsPath}", logsPath);
Log.Information("========================================");

app.Run();
