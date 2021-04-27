using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Dynamically creates the select statement for the query provider
    /// based on the query
    /// </summary>
    public class EFQueryFieldsVisitor<TResult> : IQueryVisitor<TResult>
        where TResult : class
    {
        private static readonly IEnumerable<string> fields = typeof(TResult)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(prop =>
                prop.PropertyType.GetCustomAttribute<DataContractAttribute>() == null &&
                !prop.PropertyType.IsGenericIEnumerable()
            )
            .Select(p => p.Name);
        private static readonly ConcurrentDictionary<IEnumerable<string>, Expression<Func<TResult, TResult>>> expressionCache = new();

        private readonly IQuery query;

        public EFQueryFieldsVisitor(IQuery query) => this.query = query.NotNull().Expect(nameof(query));

        public IQueryable<TResult> Visit(IQueryable<TResult> queryable)
        {
            var acceptedFields = fields.Intersect(query.Fields, StringComparer.OrdinalIgnoreCase);

            return acceptedFields.Any()
                ? QueryFields(queryable, acceptedFields)
                : queryable;
        }

        private static IQueryable<TResult> QueryFields(IQueryable<TResult> query, IEnumerable<string> fields)
        {
            if (expressionCache.TryGetValue(fields, out var selector))
            {
                return query.Select(selector);
            }

            var parameter = Expression.Parameter(typeof(TResult), "e");
            var bindings = fields
                .Select(name => Expression.PropertyOrField(parameter, name))
                .Select(member => Expression.Bind(member.Member, member));
            var body = Expression.MemberInit(Expression.New(typeof(TResult)), bindings);
            selector = Expression.Lambda<Func<TResult, TResult>>(body, parameter);
            _ = expressionCache.TryAdd(fields, selector);
            return query.Select(selector);
        }
    }
}
