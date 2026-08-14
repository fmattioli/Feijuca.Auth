namespace Feijuca.Auth.Models;

public class PagedResult<T>
{
    public PagedResult() => Results = [];

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (TotalResults + PageSize - 1) / PageSize;
    public int TotalResults { get; set; }
    public IEnumerable<T> Results { get; set; }
}