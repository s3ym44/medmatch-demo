using System.Text.Json;
using MedMatch.Application.Common;
using MedMatch.Domain.Common;

namespace MedMatch.Api.Common;

/// <summary>Domain/uygulama hatalarını uygun HTTP durum kodlarına çevirir.</summary>
public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next; _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await Write(context, MapStatus(ex.Type), ex.Message);
        }
        catch (DomainException ex)
        {
            await Write(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmeyen hata");
            await Write(context, StatusCodes.Status500InternalServerError, "Sunucu hatası.");
        }
    }

    private static int MapStatus(AppErrorType type) => type switch
    {
        AppErrorType.Validation => StatusCodes.Status400BadRequest,
        AppErrorType.NotFound => StatusCodes.Status404NotFound,
        AppErrorType.Conflict => StatusCodes.Status409Conflict,
        AppErrorType.Forbidden => StatusCodes.Status403Forbidden,
        AppErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status400BadRequest
    };

    private static async Task Write(HttpContext context, int status, string message)
    {
        if (context.Response.HasStarted) return;
        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}
