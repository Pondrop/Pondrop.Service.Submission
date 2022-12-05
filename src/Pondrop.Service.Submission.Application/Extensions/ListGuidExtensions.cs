namespace Pondrop.Service.Submission.Application.Extensions;

public static class ListGuidExtensions
{
    public static string ToIdQueryString(this IList<Guid>? guids)
        => guids?.Any() == true
            ? string.Join(',', guids.Select(s => $"'{s}'"))
            : string.Empty;
}