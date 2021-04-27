using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BusinessApp.Kernel;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Runs Entity Framework specific logic based on the <see cref="IQuery"/> data
    /// </summary>
    public class EFQuerySortVisitor<TResult> : IQueryVisitor<TResult>
        where TResult : class
    {
        private const char DescendingChar = '-';
        private readonly IQuery query;
        private static readonly IEnumerable<string> includablePropNames = typeof(TResult)
           .GetProperties(BindingFlags.Instance | BindingFlags.Public)
           .Select(p => p.Name)
           .ToList();
        private static readonly ConcurrentDictionary<string, Expression<Func<TResult, object>>> expressionCache = new();

        public EFQuerySortVisitor(IQuery query) => this.query = query.NotNull().Expect(nameof(query));

        public IQueryable<TResult> Visit(IQueryable<TResult> queryable)
        {
            var knownFields = query.Sort
                .ToDictionary(field => field[0] != DescendingChar, field => field)
                .Select(kvp =>
                    new KeyValuePair<bool, string>(
                        kvp.Key,
                        kvp.Key ? kvp.Value.ConvertToPascalCase() : kvp.Value[1..].ConvertToPascalCase())
                    )
                .Where(kvp => includablePropNames.Contains(kvp.Value.Split('.')[0]));

            foreach (var sortabaleKvp in knownFields)
            {
                queryable = !sortabaleKvp.Key
                    ? queryable.OrderByDescending(CreateSortExpression(sortabaleKvp.Value))
                    : queryable.OrderBy(CreateSortExpression(sortabaleKvp.Value));
            }

            return queryable;
        }

        private static Expression<Func<TResult, object>> CreateSortExpression(string fieldName)
        {
            if (expressionCache.TryGetValue(fieldName, out var selector))
            {
                return selector;
            }

            var param = Expression.Parameter(typeof(TResult), "f");
            var conversion = Expression.Convert(
                Expression.Property(param, fieldName),
                typeof(object));
            var lambda = Expression.Lambda<Func<TResult, object>>(conversion, param);
            _ = expressionCache.TryAdd(fieldName, lambda);

            return lambda;
        }
    }
}
