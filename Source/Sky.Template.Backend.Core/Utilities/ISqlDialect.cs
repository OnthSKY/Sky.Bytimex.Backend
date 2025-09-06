namespace Sky.Template.Backend.Core.Utilities;
public interface ISqlDialect
{
    string ParameterPrefix { get; }
    string Quote(string identifier);
    string LikeOperator(bool caseInsensitive);
    string Paginate(int page, int pageSize);
    string Top(int top);
    string StripOrderBy(string sql);
    string StripPaging(string sql);               // NEW
    string CountWrap(string sql);
    string FormatLike(string column, string parameter, bool caseInsensitive); // optional, ama ekledim
}
