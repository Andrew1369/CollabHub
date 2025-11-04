using System.Linq.Expressions;
using System.Reflection;

namespace CollabHub.Utils;

public static class QueryableExtensions
{
    public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string? sortBy, string? sortDir)
    {
        if (string.IsNullOrWhiteSpace(sortBy)) return source;
        var prop = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (prop is null) return source;

        var param = Expression.Parameter(typeof(T), "x");
        var body = Expression.Property(param, prop);
        var keySelector = Expression.Lambda(body, param);

        var method = (sortDir?.ToLowerInvariant() == "desc") ? "OrderByDescending" : "OrderBy";
        var call = Expression.Call(typeof(Queryable), method, new Type[] { typeof(T), prop.PropertyType },
                                   source.Expression, Expression.Quote(keySelector));
        return source.Provider.CreateQuery<T>(call);
    }
}
