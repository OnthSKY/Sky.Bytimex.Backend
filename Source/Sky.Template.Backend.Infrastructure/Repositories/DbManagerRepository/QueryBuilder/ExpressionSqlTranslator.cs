using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.Sql;

namespace Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.QueryBuilder;

public class ExpressionSqlTranslator
{
    private readonly ISqlDialect _dialect;
    public ExpressionSqlTranslator(ISqlDialect dialect) => _dialect = dialect;

    public (string Sql, List<object?> Parameters) Translate<T>(Expression<Func<T, bool>> expression, Func<string, string> columnResolver)
    {
        _params.Clear();
        var sql = Visit(expression.Body, columnResolver);
        return (sql, new List<object?>(_params));
    }

    private readonly List<object?> _params = new();

    private string Visit(Expression exp, Func<string, string> columnResolver)
    {
        return exp.NodeType switch
        {
            ExpressionType.AndAlso => Combine((BinaryExpression)exp, "AND", columnResolver),
            ExpressionType.OrElse => Combine((BinaryExpression)exp, "OR", columnResolver),
            ExpressionType.Equal => Compare((BinaryExpression)exp, "=", columnResolver),
            ExpressionType.NotEqual => Compare((BinaryExpression)exp, "<>", columnResolver),
            ExpressionType.GreaterThan => Compare((BinaryExpression)exp, ">", columnResolver),
            ExpressionType.GreaterThanOrEqual => Compare((BinaryExpression)exp, ">=", columnResolver),
            ExpressionType.LessThan => Compare((BinaryExpression)exp, "<", columnResolver),
            ExpressionType.LessThanOrEqual => Compare((BinaryExpression)exp, "<=", columnResolver),
            ExpressionType.Not => $"NOT {Visit(((UnaryExpression)exp).Operand, columnResolver)}",
            ExpressionType.Call => Method((MethodCallExpression)exp, columnResolver),
            ExpressionType.MemberAccess => Member((MemberExpression)exp, columnResolver),
            ExpressionType.Constant => Constant((ConstantExpression)exp),
            _ => throw new NotSupportedException("ExpressionNotSupported")
        };
    }

    private string Combine(BinaryExpression b, string op, Func<string, string> columnResolver)
        => $"({Visit(b.Left, columnResolver)} {op} {Visit(b.Right, columnResolver)})";

    private string Compare(BinaryExpression b, string op, Func<string, string> columnResolver)
    {
        var left = Visit(b.Left, columnResolver);
        var right = Visit(b.Right, columnResolver);
        return $"{left} {op} {right}";
    }

    private string Method(MethodCallExpression m, Func<string, string> columnResolver)
    {
        if (m.Method.Name == "Contains" && m.Object != null && m.Object.Type == typeof(string))
        {
            var col = Visit(m.Object, columnResolver);
            var val = Evaluate(m.Arguments[0])?.ToString() ?? string.Empty;
            val = EscapeLike(val);
            var param = AddParam($"%{val}%");
            return $"{col} {_dialect.LikeOperator(false)} {param}";
        }
        if (m.Method.Name == "StartsWith" && m.Object != null)
        {
            var col = Visit(m.Object, columnResolver);
            var val = EscapeLike(Evaluate(m.Arguments[0])?.ToString() ?? string.Empty);
            var param = AddParam($"{val}%");
            return $"{col} {_dialect.LikeOperator(false)} {param}";
        }
        if (m.Method.Name == "EndsWith" && m.Object != null)
        {
            var col = Visit(m.Object, columnResolver);
            var val = EscapeLike(Evaluate(m.Arguments[0])?.ToString() ?? string.Empty);
            var param = AddParam($"%{val}");
            return $"{col} {_dialect.LikeOperator(false)} {param}";
        }
        if (m.Method.Name == "Contains" && m.Object == null && m.Arguments.Count == 2)
        {
            var collection = Evaluate(m.Arguments[0]) as IEnumerable;
            if (collection == null) throw new NotSupportedException("ExpressionNotSupported");
            var col = Visit(m.Arguments[1], columnResolver);
            var list = new List<string>();
            foreach (var item in collection)
            {
                list.Add(AddParam(item));
            }
            return $"{col} IN ({string.Join(",", list)})";
        }
        throw new NotSupportedException("ExpressionNotSupported");
    }

    private string Member(MemberExpression m, Func<string, string> columnResolver)
    {
        if (m.Expression is ParameterExpression)
            return columnResolver(m.Member.Name);
        var value = Evaluate(m);
        return AddParam(value);
    }

    private string Constant(ConstantExpression c) => AddParam(c.Value);

    private object? Evaluate(Expression exp)
        => Expression.Lambda(exp).Compile().DynamicInvoke();

    private string AddParam(object? value)
    {
        var name = $"{_dialect.ParameterPrefix}p{_params.Count}";
        _params.Add(value);
        return name;
    }

    private static string EscapeLike(string value)
        => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
