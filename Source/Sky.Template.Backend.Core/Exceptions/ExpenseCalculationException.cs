namespace Sky.Template.Backend.Core.Exceptions;

public class ExpenseCalculationException : BusinessRulesException
{
    public ExpenseCalculationException(string resourceKey, params object[] formatArgs) 
        : base(resourceKey, formatArgs)
    {
    }
} 
