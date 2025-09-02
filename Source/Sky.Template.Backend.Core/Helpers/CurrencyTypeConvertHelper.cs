using System.Globalization;
using System.Linq;
using Sky.Template.Backend.Core.Exceptions;

namespace Sky.Template.Backend.Core.Helpers;

public static class CurrencyTypeConvertHelper
{
    public static decimal ConvertToDecimal(string amount, string? currency, int? expenseArea = 1)
    {
        if (string.IsNullOrWhiteSpace(amount))
            return 0.00m;

        if (string.IsNullOrWhiteSpace(currency))
        {
            if (expenseArea == 1)
            {
                currency = "TRY";
            }
            else
            {
                throw new BusinessRulesException("CurrencyRequired");
            }
        }

        var upperCurrency = currency.ToUpperInvariant();

        ValidateFormat(amount, upperCurrency);

        var normalized = NormalizeAmount(amount, upperCurrency);

        if (decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        throw new BusinessRulesException("InvalidAmountFormat");
    }

    public static string ConvertToInvariantDecimalString(string amount, string? currency, int? expenseArea = 1)
    {
        var decimalValue = ConvertToDecimal(amount, currency, expenseArea);
        return decimalValue.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string NormalizeAmount(string amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(amount))
            return "0.00";

        switch (currency)
        {
            case "TRY":
            case "BRL":
            case "CHF":
            case "EUR":
                amount = amount.Replace(".", string.Empty).Replace(",", ".");
                break;

            case "USD":
            case "GBP":
            case "CAD":
            case "AUD":
                amount = amount.Replace(",", string.Empty);
                break;

            case "JPY":
                amount = amount.Replace(",", string.Empty).Replace(".", string.Empty);
                break;

            default:
                throw new BusinessRulesException("UnsupportedCurrency");
        }

        return amount;
    }

    private static void ValidateFormat(string amount, string currency)
    {
        switch (currency)
        {
            case "TRY":
            case "BRL":
            case "CHF":
            case "EUR":
                if (amount.Count(c => c == ',') > 1)
                    throw new BusinessRulesException("InvalidAmountFormat");
                var commaIndex = amount.IndexOf(',');
                if (commaIndex >= 0 && amount[(commaIndex + 1)..].Any(c => !char.IsDigit(c)))
                    throw new BusinessRulesException("InvalidAmountFormat");
                break;
            case "USD":
            case "GBP":
            case "CAD":
            case "AUD":
                if (amount.Count(c => c == '.') > 1)
                    throw new BusinessRulesException("InvalidAmountFormat");
                var dotIndex = amount.IndexOf('.');
                if (dotIndex >= 0 && amount[(dotIndex + 1)..].Any(c => !char.IsDigit(c)))
                    throw new BusinessRulesException("InvalidAmountFormat");
                break;
            case "JPY":
                if (amount.Contains('.') || amount.Count(c => c == ',') > 1)
                    throw new BusinessRulesException("InvalidAmountFormat");
                break;
        }
    }
}

