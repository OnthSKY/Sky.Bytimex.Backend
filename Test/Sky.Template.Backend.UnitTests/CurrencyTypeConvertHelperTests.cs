using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Exceptions;
using Xunit;

namespace Sky.Template.Backend.UnitTests
{
    public class CurrencyTypeConvertHelperTests
    {
        [Theory]
        [InlineData("1.000,50", "TRY", 1, 1000.50)]     // Türk Lirası (Avrupa format)
        [InlineData("1.234,56", "EUR", 1, 1234.56)]     // Euro (Avrupa format)
        [InlineData("2,345.67", "USD", 2, 2345.67)]     // Dolar (İngiliz format)
        [InlineData("1,000.50", "GBP", 2, 1000.50)]     // Pound (İngiliz format)
        [InlineData("123456", "JPY", 1, 123456)]        // JPY (ondalıklı değil)
        [InlineData("1000.50", null, 1, 1000.50)]       // currency null ama expenseArea 1 → TRY kabul edilir
        [InlineData("1000.50", "CHF", 1, 1000.50)]      // İsviçre Frangı (Avrupa format)

        public void ConvertToDecimal_ValidInputs_ReturnsExpected(string amount, string? currency, int expenseArea, decimal expected)
        {
            var result = CurrencyTypeConvertHelper.ConvertToDecimal(amount, currency, expenseArea);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertToDecimal_EmptyAmount_ReturnsZero()
        {
            var result = CurrencyTypeConvertHelper.ConvertToDecimal("", "TRY", 1);
            Assert.Equal(0.00m, result);
        }

        [Fact]
        public void ConvertToDecimal_CurrencyAndExpenseAreaNull_ThrowsBusinessRulesException()
        {
            var exception = Assert.Throws<BusinessRulesException>(() =>
                CurrencyTypeConvertHelper.ConvertToDecimal("1234.56", null, null));

            Assert.Equal("CurrencyRequired", exception.Message);
        }

        [Fact]
        public void ConvertToDecimal_UnknownCurrency_ThrowsBusinessRulesException()
        {
            var exception = Assert.Throws<BusinessRulesException>(() =>
                CurrencyTypeConvertHelper.ConvertToDecimal("1234.56", "XYZ", 1));

            Assert.Equal("UnsupportedCurrency", exception.Message);
        }

        [Fact]
        public void ConvertToDecimal_InvalidFormat_ThrowsBusinessRulesException()
        {
            var exception = Assert.Throws<BusinessRulesException>(() =>
                CurrencyTypeConvertHelper.ConvertToDecimal("1.000.000,00.00", "TRY", 1));

            Assert.Equal("InvalidAmountFormat", exception.Message);
        }

        [Fact]
        public void ConvertToInvariantDecimalString_ReturnsExpectedFormat()
        {
            var result = CurrencyTypeConvertHelper.ConvertToInvariantDecimalString("1.234,56", "EUR", 1);
            Assert.Equal("1234.56", result);
        }
    }
}
