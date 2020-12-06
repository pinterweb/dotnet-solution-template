namespace BusinessApp.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Runs Entity Framework specific logic based on the <see cref="IQuery"/> data
    /// </summary>
    public class EFQuerySortVisitor<TResult> : IQueryVisitor<TResult>
        where TResult : class
    {
        private const char DescendingChar = '-';
        private readonly IQuery query;
        private static IEnumerable<string> IncludablePropNames = typeof(TResult)
           .GetProperties(BindingFlags.Instance | BindingFlags.Public)
           .Select(p => p.Name)
           .ToList();
        private static ConcurrentDictionary<string, Expression<Func<TResult, object>>> ExpressionCache
            = new ConcurrentDictionary<string, Expression<Func<TResult, object>>>();

        public EFQuerySortVisitor(IQuery query)
        {
            this.query = query.NotNull().Expect(nameof(query));
        }

        public IQueryable<TResult> Visit(IQueryable<TResult> queryable)
        {
            var knownFields = query.Sort
                .ToDictionary(field => field[0] != DescendingChar, field => field)
                .Select(kvp =>
                    new KeyValuePair<bool, string>(
                        kvp.Key,
                        (kvp.Key ? kvp.Value.ConvertToPascalCase() : kvp.Value.Substring(1).ConvertToPascalCase()))
                    )
                .Where(kvp => IncludablePropNames.Contains(kvp.Value.Split('.')[0]));

            foreach (var sortabaleKvp in knownFields)
            {
                if (!sortabaleKvp.Key)
                {
                    queryable = queryable.OrderByDescending(CreateSortExpression(sortabaleKvp.Value));
                }
                else
                {
                    queryable = queryable.OrderBy(CreateSortExpression(sortabaleKvp.Value));
                }
            }

            return queryable;
        }

        private static Expression<Func<TResult, object>> CreateSortExpression(string fieldName)
        {
            if (ExpressionCache.TryGetValue(fieldName, out Expression<Func<TResult, object>> selector))
            {
                return selector;
            }

            var param = Expression.Parameter(typeof(TResult), "f");
            var prop = Expression.Property(param, fieldName);
            var conversion = Expression.Convert(
                Expression.Property(param, fieldName),
                typeof(object));
            var lambda =  Expression.Lambda<Func<TResult, object>>(conversion, param);
            ExpressionCache.TryAdd(fieldName, lambda);

            return lambda;
        }
    }
}
