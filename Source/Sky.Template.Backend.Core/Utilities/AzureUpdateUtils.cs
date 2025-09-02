using System.Text.RegularExpressions;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Core.Utilities;


public static class AzureUpdateUtils
{
    #region Category Mapping
    public static string MapExpenseCategoryType(Dictionary<string, List<string>> categoryDictionary, string inputCategoryType, string inputMerchantName)
    {
        var keywords = new HashSet<string>
        {
            "VE", "ILE", "VEYA", "AMA", "FAKAT", "LAKIN", "SU HALDE", "AYNI ZAMANDA",
            "BUNUNLA BIRLIKTE", "KURUMU", "VAKFI", "DERNEGI", "SIRKETI", "A.S.", "A�",
            "LTD. STI.", "STI.", "ANONIM SIRKET", "LIMITED SIRKET", "AND", "OR", "COMPANY"
        };

        var normalizedInputCategoryType = Utils.Normalize(inputCategoryType);
        var normalizedInputMerchantName = Utils.Normalize(inputMerchantName);

        foreach (var key in categoryDictionary.Keys)
        {
            string normalizedKey = Utils.Normalize(key);

            if (normalizedInputCategoryType == normalizedKey || normalizedInputMerchantName.Contains(normalizedKey))
            {
                return key;
            }
        }

        string[] inputCategoryWordsArr = SplitAndNormalize(normalizedInputCategoryType);
        string[] inputMerchantWordsArr = SplitAndNormalize(normalizedInputMerchantName);

        var bestMatchKey = string.Empty;
        var bestMatchScore = 0;

        foreach (var CategoryDictionaryItem in categoryDictionary)
        {
            var matchScore = CalculateMatchScore(CategoryDictionaryItem.Value.ToArray(), inputCategoryWordsArr, inputMerchantWordsArr, keywords);

            if (matchScore > bestMatchScore)
            {
                bestMatchScore = matchScore;
                bestMatchKey = CategoryDictionaryItem.Key;
            }
        }

        return bestMatchKey;
    }

    private static string[] SplitAndNormalize(string input)
    {
        return input.Split(new[] { ' ', ',', '-', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private static int CalculateMatchScore(string[] categoryDictionaryItemValueArr, string[] inputCategoriesArr, string[] merchantWordsArr, HashSet<string> notScoreKeywords)
    {
        int score = 0;
        foreach (var dictionary in categoryDictionaryItemValueArr)
        {
            if (notScoreKeywords.Contains(dictionary)) continue;

            foreach (var categoryWord in inputCategoriesArr)
            {
                if (Utils.Normalize(dictionary).StartsWith(categoryWord))
                {
                    score += 3;
                }
            }

            foreach (var merchantWord in merchantWordsArr)
            {
                if (Utils.Normalize(dictionary).StartsWith(merchantWord))
                {
                    score++;
                }
            }
        }
        return score;
    }

    #endregion
    public static string MapCurrency(string inputCurrency)
    {
        var currencyMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "TRY",  new () {"TRY", "T�rk Liras�", "TL", "?" , "*"} },
            { "USD", new () { "USD", "DOLAR", "Dolar", "$", "US Dollar", "American Dollar" } },
            { "EUR", new () { "EUR", "EURO", "�", "Euro" } },
            { "GBP", new () { "GBP", "Pound", "�", "Sterling", "British Pound", "UK Pound" } },
            { "JPY", new () { "JPY", "YEN", "�", "Japanese Yen" } },
            { "AUD", new () { "AUD", "Australian Dollar", "A$", "AUS Dollar" } },
            { "CAD", new () { "CAD", "Canadian Dollar", "C$", "CAD Dollar" } },
            { "CHF", new () { "CHF", "Swiss Franc", "Fr", "Swiss" } },
            { "CNY", new () { "CNY", "RMB", "�", "Chinese Yuan", "Yuan" } },
            { "RUB", new () { "RUB", "Ruble", "?", "Russian Ruble" } },
            { "INR", new () { "INR", "?", "Indian Rupee", "Rupee" } },
            { "BRL", new () { "BRL", "Real", "R$", "Brazilian Real" } },
            { "MXN", new () { "MXN", "Mexican Peso", "$", "Mex Peso" } },
            { "ZAR", new () { "ZAR", "Rand", "South African Rand", "R" } }
        };

        foreach (var kvp in currencyMap)
        {
            if (kvp.Value.Contains(inputCurrency, StringComparer.OrdinalIgnoreCase))
            {
                return kvp.Key;
            }
        }

        return string.Empty;
    }
    public static string MapPaymentType(string inputPaymentType)
    {
        var paymentTypeMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "CASH", new () { "NAK�T", "CASH", "Pe�in", "Haz�r Para", "Hand Cash", "Direct Cash" } },
            { "CREDIT_CARD", new () { "Visa", "MasterCard", "Credit Card", "EFT", "POS", "Post", "Kart", "VISA", "MASTERCARD", "Kredi Kart�", "TROY", "AMEX", "American Express" } },
            { "OTHER", new () { "Di�er", "Bitcoin", "BTC", "Kripto", "Crypto", "PayPal", "Square", "Stripe", "Alternative", "Di�er �deme" } }
        };

        foreach (var kvp in paymentTypeMap)
        {
            if (kvp.Value.Any(value => inputPaymentType.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return kvp.Key;
            }
        }


        return "OTHER";
    }
    public static string GetClosestTaxRate(string currency, string taxRate, string itemPrice, List<int> taxRates)
    {
        if (string.IsNullOrEmpty(taxRate))
        {
            return "0";
        }

        // Tax rate'i temizle
        taxRate = !string.IsNullOrEmpty(taxRate)
            ? Regex.Replace(taxRate, @"\D", "")
            : taxRate;

        taxRate = taxRate == "01" ? "1" : taxRate;

        // Temizlendikten sonra bo� mu kontrol et
        if (string.IsNullOrEmpty(taxRate))
        {
            return "0";
        }

        // Int'e �evirmeyi dene
        if (int.TryParse(taxRate, out int parsedRate))
        {
            // Ge�erli range'de mi kontrol et
            if (parsedRate >= 0 && parsedRate <= 100)
            {
                // En yak�n valid tax rate'i bul
                if (taxRates?.Any() == true)
                {
                    var closestRate = taxRates.OrderBy(t => Math.Abs(t - parsedRate)).First();
                    return closestRate.ToString();
                }
            }
        }

        return "0";
    }

    public static string CalculateTaxRateFromPrice(string currency, string itemPrice, string itemTaxRate, List<int> taxRates)
    {
        string taxRate = "0";

        try
        {
            if (string.IsNullOrEmpty(itemPrice) || !taxRates?.Any() == true)
            {
                return "0";
            }

            decimal price = MoneyUtils.ConvertToDecimal(itemPrice, currency);

            // Tax rate verilmi�se kullan
            if (!string.IsNullOrEmpty(itemTaxRate))
            {
                itemTaxRate = Regex.Replace(itemTaxRate, @"\D", "");
                if (int.TryParse(itemTaxRate, out int rate) && rate >= 0 && rate <= 100)
                {
                    // En yak�n ge�erli rate'i bul
                    var closestRate = taxRates.OrderBy(t => Math.Abs(t - rate)).First();
                    return closestRate.ToString();
                }
            }

            // Tax rate yoksa varsay�lan en yayg�n rate'i d�nd�r
            return taxRates.OrderByDescending(t => taxRates.Count(r => r == t)).First().ToString();
        }
        catch (FormatException)
        {
            taxRate = "0";
        }
        catch (Exception)
        {
            taxRate = "0";
        }

        return taxRate;
    }

    public static string CalculateTaxAmount(string currency, string itemPrice, string taxRate)
    {
        try
        {
            if (string.IsNullOrEmpty(itemPrice) || string.IsNullOrEmpty(taxRate))
            {
                return "0";
            }

            decimal price = MoneyUtils.ConvertToDecimal(itemPrice, currency);

            // Tax rate'i temizle ve decimal'e �evir
            string cleanTaxRate = Regex.Replace(taxRate, @"\D", "");
            if (decimal.TryParse(cleanTaxRate, out decimal rate))
            {
                // Tax amount = (price * taxRate) / 100
                decimal taxAmount = price * rate / 100;
                return MoneyUtils.ConvertToInvariantDecimalString(taxAmount.ToString(), currency, 1);
            }
        }
        catch (Exception)
        {
            return "0";
        }

        return "0";
    }

   
    public static string ExtractFirstWordIfContainsTaxOffice(string text, string expectedText)
    {
        if (text.ToUpper(new System.Globalization.CultureInfo("tr-TR")).Contains(expectedText))
        {
            return text.Split(' ')[0];
        }
        return string.Empty;
    }
}
