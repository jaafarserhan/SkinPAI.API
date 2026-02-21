using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Serilog.Context;

namespace SkinPAI.API.Middleware;

/// <summary>
/// Middleware for comprehensive request/response logging with user context.
/// Logs are stored as: SkinPAI-{Date}.log with user identification in each entry.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate unique identifiers for this request
        var requestId = Guid.NewGuid().ToString("N")[..12];
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString("N")[..8];
        
        // Extract user information (will be "Anonymous" before authentication)
        var userId = GetUserId(context);
        var userEmail = GetUserEmail(context);
        var userType = GetUserType(context);

        // Add context properties for all subsequent log entries in this request
        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserEmail", userEmail))
        using (LogContext.PushProperty("UserType", userType))
        using (LogContext.PushProperty("ClientIP", GetClientIP(context)))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString()))
        {
            // Start timing
            var stopwatch = Stopwatch.StartNew();
            var requestTime = DateTime.UtcNow;

            // Log request details
            _logger.LogInformation(
                "➡️ REQUEST START | {Method} {Path}{QueryString} | User: {UserId} ({UserType}) | IP: {ClientIP}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                userId,
                userType,
                GetClientIP(context));

            // Log request body for POST/PUT/PATCH (excluding sensitive data)
            if (ShouldLogRequestBody(context))
            {
                var requestBody = await ReadRequestBodyAsync(context);
                if (!string.IsNullOrEmpty(requestBody))
                {
                    var sanitizedBody = SanitizeRequestBody(requestBody);
                    _logger.LogDebug("📋 REQUEST BODY | {RequestBody}", sanitizedBody);
                }
            }

            try
            {
                // Continue processing
                await _next(context);

                stopwatch.Stop();

                // Re-fetch user info after authentication middleware has run
                var authenticatedUserId = GetUserId(context);
                var authenticatedUserEmail = GetUserEmail(context);
                var authenticatedUserType = GetUserType(context);

                // Log response
                var statusCode = context.Response.StatusCode;
                var statusCategory = GetStatusCategory(statusCode);
                
                if (statusCode >= 400)
                {
                    _logger.LogWarning(
                        "⚠️ REQUEST END | {Method} {Path} | Status: {StatusCode} ({StatusCategory}) | Duration: {Duration}ms | User: {UserId} ({UserEmail})",
                        context.Request.Method,
                        context.Request.Path,
                        statusCode,
                        statusCategory,
                        stopwatch.ElapsedMilliseconds,
                        authenticatedUserId,
                        authenticatedUserEmail);
                }
                else
                {
                    _logger.LogInformation(
                        "✅ REQUEST END | {Method} {Path} | Status: {StatusCode} ({StatusCategory}) | Duration: {Duration}ms | User: {UserId}",
                        context.Request.Method,
                        context.Request.Path,
                        statusCode,
                        statusCategory,
                        stopwatch.ElapsedMilliseconds,
                        authenticatedUserId);
                }

                // Log slow requests
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    _logger.LogWarning(
                        "🐌 SLOW REQUEST | {Method} {Path} took {Duration}ms | User: {UserId}",
                        context.Request.Method,
                        context.Request.Path,
                        stopwatch.ElapsedMilliseconds,
                        authenticatedUserId);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex,
                    "❌ REQUEST FAILED | {Method} {Path} | Duration: {Duration}ms | User: {UserId} | Error: {ErrorMessage}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    GetUserId(context),
                    ex.Message);

                throw;
            }
        }
    }

    private static string GetUserId(HttpContext context)
    {
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User?.FindFirst("sub")?.Value
            ?? context.User?.FindFirst("userId")?.Value;
        
        return string.IsNullOrEmpty(userIdClaim) ? "Anonymous" : userIdClaim;
    }

    private static string GetUserEmail(HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.Email)?.Value
            ?? context.User?.FindFirst("email")?.Value
            ?? "N/A";
    }

    private static string GetUserType(HttpContext context)
    {
        return context.User?.FindFirst("membershipType")?.Value
            ?? context.User?.FindFirst("type")?.Value
            ?? "Unknown";
    }

    private static string GetClientIP(HttpContext context)
    {
        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private static bool ShouldLogRequestBody(HttpContext context)
    {
        var method = context.Request.Method;
        return method == "POST" || method == "PUT" || method == "PATCH";
    }

    private async Task<string> ReadRequestBodyAsync(HttpContext context)
    {
        try
        {
            // Enable buffering so the body can be read multiple times
            context.Request.EnableBuffering();

            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            
            // Reset the stream position
            context.Request.Body.Position = 0;

            // Limit logged body size
            if (body.Length > 2000)
            {
                return body[..2000] + "... [TRUNCATED]";
            }

            return body;
        }
        catch
        {
            return "[Unable to read request body]";
        }
    }

    private static string SanitizeRequestBody(string body)
    {
        // Remove sensitive fields from logged data
        var sensitiveFields = new[] { "password", "passwordHash", "token", "refreshToken", "accessToken", "secret", "apiKey" };
        
        foreach (var field in sensitiveFields)
        {
            // Simple regex-like replacement for JSON
            body = System.Text.RegularExpressions.Regex.Replace(
                body,
                $@"""{field}""\s*:\s*""[^""]*""",
                $@"""{field}"":""[REDACTED]""",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return body;
    }

    private static string GetStatusCategory(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => "Success",
            >= 300 and < 400 => "Redirect",
            >= 400 and < 500 => "Client Error",
            >= 500 => "Server Error",
            _ => "Unknown"
        };
    }
}

/// <summary>
/// Extension methods for registering the logging middleware.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
