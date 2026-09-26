namespace MedMatch.Application.Common;

/// <summary>Uygulama seviyesi hata (çakışma, yetki, bulunamadı vb.).</summary>
public sealed class AppException : Exception
{
    public AppErrorType Type { get; }
    public AppException(AppErrorType type, string message) : base(message) => Type = type;

    public static AppException NotFound(string m) => new(AppErrorType.NotFound, m);
    public static AppException Conflict(string m) => new(AppErrorType.Conflict, m);
    public static AppException Forbidden(string m) => new(AppErrorType.Forbidden, m);
    public static AppException Validation(string m) => new(AppErrorType.Validation, m);
    public static AppException Unauthorized(string m) => new(AppErrorType.Unauthorized, m);
}

public enum AppErrorType { Validation, NotFound, Conflict, Forbidden, Unauthorized }
