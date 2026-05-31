using HRSystem.Business.DTOs;
using HRSystem.Data.Common;

namespace HRSystem.Business.Mapping;

internal static class PagedResultMapper
{
    public static PagedResult<T> Map<T>(PagedList<T> source) =>
        new()
        {
            Items = source.Items,
            Page = source.Page,
            PageSize = source.PageSize,
            TotalCount = source.TotalCount
        };

    public static PagedResult<TDestination> Map<TSource, TDestination>(
        PagedList<TSource> source,
        Func<TSource, TDestination> mapper) =>
        new()
        {
            Items = source.Items.Select(mapper).ToList(),
            Page = source.Page,
            PageSize = source.PageSize,
            TotalCount = source.TotalCount
        };
}
