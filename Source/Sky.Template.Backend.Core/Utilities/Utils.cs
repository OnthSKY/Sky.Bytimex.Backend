using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Sky.Template.Backend.Core.Utilities;

public static class Utils
{
    public static string GetTimeFormat()
    {
        return $"{DateTime.Now.TimeOfDay.Hours}:{DateTime.Now.TimeOfDay.Minutes}:{DateTime.Now.TimeOfDay.Seconds}";
    }

    public static string GetTimeFormatForFileName()
    {
        return $"{DateTime.Now:HH-mm-ss}";
    }

    public static IConfiguration GetConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var settingsName = "appsettings.json";

        if (environment == "Development")
            settingsName = "appsettings.Development.json";

        return new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) 
            .AddJsonFile(settingsName, optional: true, reloadOnChange: true) 
            .AddEnvironmentVariables()
            .Build();
    }
    public static string StripOrderByImpl(string sql)
    {
        var upper = sql.ToUpperInvariant();
        var sb = new StringBuilder();
        int depth = 0;
        bool inString = false;
        for (int i = 0; i < upper.Length; i++)
        {
            var c = upper[i];
            if (c == '\'') inString = !inString;
            if (!inString)
            {
                if (c == '(') depth++;
                else if (c == ')') depth--;
                else if (depth == 0 && i < upper.Length - 8 && upper.Substring(i, 8) == "ORDER BY")
                {
                    return sb.ToString().TrimEnd();
                }
            }
            sb.Append(sql[i]);
        }
        return sb.ToString();
    }
    public static string ConvertToUpperEnglish(string input)
    {
        var normalizedString = input
            .Replace('�', 'c')
            .Replace('�', 'C')
            .Replace('�', 'g')
            .Replace('�', 'G')
            .Replace('�', 'i')
            .Replace('�', 'I')
            .Replace('�', 'o')
            .Replace('�', 'O')
            .Replace('�', 's')
            .Replace('�', 'S')
            .Replace('�', 'u')
            .Replace('�', 'U');

        return normalizedString.ToUpper(CultureInfo.InvariantCulture);
    }

    public static string ExtractFirstWordIfContainsTaxOffice(string text, string expectedText)
    {
        if (text.ToUpper(new System.Globalization.CultureInfo("tr-TR")).Contains(expectedText))
        {
            return text.Split(' ')[0];
        }
        return string.Empty;
    }
    public static object DbNullIfNull(object value) => value ?? DBNull.Value;

    public static string Normalize(string text)
    {
        return text.ToUpper()
            .Replace("�", "I").Replace("�", "U")
            .Replace("�", "O").Replace("�", "C")
            .Replace("�", "S").Replace("�", "G");
    }

    public static string NormalizeTurkish(string text)
    {
        return text
            .Replace("I", "�").Replace("�", "i")
            .Replace("�", "�").Replace("�", "�")
            .Replace("�", "�").Replace("�", "�")
            .Replace("�", "�").ToLower();
    }
    public static string GetMimeType(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
    public static string Slugify(string input)
    {
        string normalized = input.ToLowerInvariant()
            .Replace("�", "c")
            .Replace("�", "g")
            .Replace("�", "i")
            .Replace("�", "o")
            .Replace("�", "s")
            .Replace("�", "u");
        normalized = Regex.Replace(normalized, @"[^a-z0-9\-]", "-");  
        normalized = Regex.Replace(normalized, @"-+", "_"); // �oklu '-' -> tek '-'
        return normalized.Trim('-');
    }

}
