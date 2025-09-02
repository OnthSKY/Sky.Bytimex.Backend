using System.Runtime.CompilerServices;
using System.Text;
using Sky.Template.Backend.Core.Enums;

namespace Sky.Template.Backend.Core.Helpers;

public static class LogHelper
{
    public static async Task WriteToFileAsync(
        string message,
        LogType logType,
        string exceptionType = "General",
        string serviceName = "General",
        [CallerMemberName] string? caller = null)
    {
        try
        {
            string safeExceptionType = SanitizeFileName(exceptionType);
            string safeServiceName = SanitizeFileName(serviceName);

            string baseDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Logs",
                logType.ToString(),
                safeExceptionType,
                safeServiceName,
                DateTime.Now.Year.ToString(),
                DateTime.Now.Month.ToString("D2")
            );

            Directory.CreateDirectory(baseDirectory);

            string fileName = GenerateLogFileName(logType, safeServiceName);
            string filePath = Path.Combine(baseDirectory, fileName);

            string logMessage = $"""
                ----------
                [Timestamp]: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                [Caller]: {caller}
                [LogType]: {logType}
                [ExceptionType]: {safeExceptionType}
                [Service]: {safeServiceName}
                [Message]:
                {message}
                ----------
                """;

            await File.AppendAllTextAsync(filePath, logMessage, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOGGING ERROR]: {ex.Message}");
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "Unknown";

        fileName = Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c, '_'));
        return fileName.Replace(" ", "_");
    }

    private static string GenerateLogFileName(LogType type, string servicePath)
    {
        string safeServicePath = SanitizeFileName(servicePath.Replace("/", "_").Replace("\\", "_"));
        string dayPart = DateTime.Now.ToString("yyyy-MM-dd");
        return $"{dayPart}_{type}_{safeServicePath}.txt";
    }
}
