namespace Sky.Template.Backend.Core.Exceptions;

/// <summary>
/// Sistemdeki tüm hesaplama işlemleri sırasında oluşabilecek hatalar için kullanılır
/// </summary>
public class CalculationException : BusinessRulesException
{
    public string CalculationType { get; }
    public object CalculationData { get; }

    public CalculationException(
        string resourceKey, 
        string calculationType,
        object calculationData = null,
        params object[] formatArgs) 
        : base(resourceKey, formatArgs)
    {
        CalculationType = calculationType;
        CalculationData = calculationData;
    }
} 
