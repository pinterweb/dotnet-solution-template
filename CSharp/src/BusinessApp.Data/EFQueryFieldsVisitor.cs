namespace BusinessApp.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;
    using BusinessApp.App;
    using BusinessApp.Domain;

    /// <summary>
    /// Factory to create the fields query visitor
    /// </summary>
    public class EFQueryFieldsVisitorFactory<TQuery, TResult> : IQueryVisitorFactory<TQuery, TResult>
        where TQuery : Query
        where TResult : class
    {
        public IQueryVisitor<TResult> Create(TQuery query) => new EFQueryFieldsVisitor<TResult>(query);
    }

    /// <summary>
    /// Dynamically creates the select statement for the query provider
    /// based on the query
    /// </summary>
    public class EFQueryFieldsVisitor<TResult> : IQueryVisitor<TResult>
        where TResult : class
    {
        private static IEnumerable<string> Fields = typeof(TResult)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(prop =>
                prop.PropertyType.GetCustomAttribute<DataContractAttribute>() == null &&
                !prop.PropertyType.IsGenericIEnumerable()
            )
            .Select(p => p.Name);
        private static ConcurrentDictionary<
            IEnumerable<string>,
            Expression<Func<TResult, TResult>>
        > ExpressionCache = new ConcurrentDictionary<
            IEnumerable<string>,
            Expression<Func<TResult, TResult>>>();

        private readonly Query query;

        public EFQueryFieldsVisitor(Query query)
        {
            this.query = GuardAgainst.Null(query, nameof(query));
        }

        public IQueryable<TResult> Visit(IQueryable<TResult> queryable)
        {
            var acceptedFields = Fields.Intersect(query.Fields, StringComparer.OrdinalIgnoreCase);

            if (acceptedFields.Any())
            {
                return QueryFields(queryable, acceptedFields);
            }

            return queryable;
        }

        private static IQueryable<TResult> QueryFields(IQueryable<TResult> query, IEnumerable<string> fields)
        {
            if (ExpressionCache.TryGetValue(fields, out Expression<Func<TResult, TResult>> selector))
            {
                return query.Select(selector);
            }

            var parameter = Expression.Parameter(typeof(TResult), "e");
            var bindings = fields
                .Select(name => Expression.PropertyOrField(parameter, name))
                .Select(member => Expression.Bind(member.Member, member));
            var body = Expression.MemberInit(Expression.New(typeof(TResult)), bindings);
            selector = Expression.Lambda<Func<TResult, TResult>>(body, parameter);
            ExpressionCache.TryAdd(fields, selector);
            return query.Select(selector);
        }
    }
}
