namespace Sky.Template.Backend.Core.Utilities;

public interface ISqlDialect
{
    string ParameterPrefix { get; }
    string Quote(string identifier);
    string LikeOperator(bool caseInsensitive);
    string Paginate(int page, int pageSize);
    string Top(int top);
    string StripOrderBy(string sql);
    string CountWrap(string sql);
}
