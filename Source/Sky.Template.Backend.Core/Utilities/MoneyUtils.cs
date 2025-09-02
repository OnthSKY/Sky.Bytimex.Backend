using System.Globalization;

namespace Sky.Template.Backend.Core.Utilities;

public static class MoneyUtils
{
    #region Decimal Format Convert
    public static string ConvertToInvariantDecimalString(string amount, string currency, int expenseArea = 1)
    {
        if (string.IsNullOrEmpty(amount))
        {
            return "0.00";
        }
        if (string.IsNullOrEmpty(currency) && expenseArea == 1)
        {
            currency = "TRY";
        }

        string result = amount;

        switch (currency ?? "".ToUpperInvariant())
        {
            case "TRY":
            case "BRL":
            case "CHF":
                result = NormalizeAmountForTRY(amount);
                break;

            case "EUR":
            case "USD":
            case "GBP":
            case "CAD":
            case "AUD":
                result = NormalizeAmountForEURUSD(amount);
                break;

            case "JPY":
                result = amount.Replace(",", "").Replace(".", "");
                break;

            default:
                return amount;
        }
        if (decimal.TryParse(result, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out decimal parsedAmount))
        {
            return parsedAmount.ToString("0.00", CultureInfo.InvariantCulture);
        }

        return result;
    }

    private static string NormalizeAmountForTRY(string amount)
    {
        if (amount.Contains(","))
        {
            amount = amount.Replace(",", ".");
        }

        if (amount.Contains("."))
        {
            int lastDotIndex = amount.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                string beforeDot = amount.Substring(0, lastDotIndex);
                string afterDot = amount.Substring(lastDotIndex + 1);

                beforeDot = beforeDot.Replace(".", "");
                amount = beforeDot + "." + afterDot;
            }
        }

        return amount;
    }

    private static string NormalizeAmountForEURUSD(string amount)
    {
        if (amount.Contains(","))
        {
            amount = amount.Replace(",", ".");
        }

        return amount;
    }

    #endregion
    public static decimal ConvertToDecimal(string amount, string currency, int expenseArea = 1)
    {
        if (string.IsNullOrEmpty(amount))
        {
            return 0.00m;
        }

        if (string.IsNullOrEmpty(currency) && expenseArea == 1)
        {
            currency = "TRY";
        }

        string normalizedAmount = amount;

        switch ((currency ?? "").ToUpperInvariant())
        {
            case "TRY":
            case "BRL":
            case "CHF":
                normalizedAmount = NormalizeAmountForTRY(amount);
                break;

            case "EUR":
            case "USD":
            case "GBP":
            case "CAD":
            case "AUD":
                normalizedAmount = NormalizeAmountForEURUSD(amount);
                break;

            case "JPY":
                normalizedAmount = amount.Replace(",", "").Replace(".", "");
                break;

            default:
                normalizedAmount = amount;
                break;
        }

        if (decimal.TryParse(normalizedAmount, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out decimal parsedAmount))
        {
            return parsedAmount;
        }

        throw new FormatException($"Geçersiz miktar formatý: {amount}");
    }

}
