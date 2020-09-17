namespace BusinessApp.Data
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using BusinessApp.App;
    using BusinessApp.Domain;

    public class QueryOperatorSpecificationBuilder<TQuery, TContract> :
        ILinqSpecificationBuilder<TQuery, TContract>
    {
        protected static readonly ICollection<PropertyInfo> Filters =
            typeof(TQuery)
            .GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(QueryOperatorAttribute)).Any())
            .ToList();

        public LinqSpecification<TContract> Build(TQuery query)
        {
            var filters = Filters.Select(p => CreateSpecFromQuery(query, p));

            return
                !filters.Any() ?
                new NullSpecification<TContract>(true) :
                filters.Aggregate((current, next) => current & next);
        }

        private static LinqSpecification<TContract> CreateSpecFromQuery(TQuery query, PropertyInfo p)
        {
            var queryPropMemberExpr = Expression.Property(Expression.Constant(query), p);

            var propertyValue = CreatePropertyAccessor(queryPropMemberExpr)
                .Compile()
                (query);

            if (propertyValue == null)
            {
                return new NullSpecification<TContract>(true);
            }

            var attr = p.GetCustomAttribute<QueryOperatorAttribute>();

            //x =>
            var contractParam = Expression.Parameter(typeof(TContract), "contract");

            var contractProp = Expression.Property(contractParam, attr.TargetProp);

            // x.Prop == "Value"
            var body = MapQueryOperator(attr, queryPropMemberExpr, p, contractProp, query);

            // x => x.LastName == "Curry"
            var lambda = Expression.Lambda<Func<TContract, bool>>(body, contractParam);

            return new LinqSpecification<TContract>(lambda);
        }

        private static Expression MapQueryOperator(QueryOperatorAttribute attribute,
            MemberExpression queryMemberExpr, PropertyInfo queryProp, MemberExpression contractProp,
            TQuery query)
        {
            var contractVal = Expression.Convert(contractProp, queryProp.PropertyType);

            switch (attribute.OperatorToUse)
            {
                case QueryOperators.Contains:
                    var c = (contractProp.Member as PropertyInfo);

                    var propertyHasValues = CreateAnyMethod(queryProp, c.PropertyType)
                        .Compile()
                        (query);

                    if (!propertyHasValues)
                    {
                        return Expression.Constant(true);
                    }

                    var method = typeof(Enumerable).
                                        GetMethods(BindingFlags.Static | BindingFlags.Public).
                                        Where(x => x.Name == "Contains").
                                        Single(x => x.GetParameters().Length == 2).
                                        MakeGenericMethod(c.PropertyType);

                    return Expression.Call(null, method, queryMemberExpr, contractProp);
                case QueryOperators.GreaterThanOrEqualTo:
                    return Expression.GreaterThanOrEqual(contractVal, queryMemberExpr);
                case QueryOperators.LessThanOrEqualTo:
                    return Expression.LessThanOrEqual(contractVal, queryMemberExpr);
                case QueryOperators.GreaterThan:
                    return Expression.GreaterThan(contractVal, queryMemberExpr);
                case QueryOperators.LessThan:
                    return Expression.LessThan(contractVal, queryMemberExpr);
                default:
                    return Expression.Equal(queryMemberExpr, contractVal);
            }
        }

        private static Expression<Func<TQuery, object>> CreatePropertyAccessor(MemberExpression memberExpr)
        {
            return Expression.Lambda<Func<TQuery, object>>(
                Expression.Convert(memberExpr, typeof(object)),
                Expression.Parameter(typeof(TQuery), "q")
            );
        }

        private static Expression<Func<TQuery, bool>> CreateAnyMethod(PropertyInfo queryPropertyInfo,
            Type contractPropertyType)
        {
            var anyMethod = typeof(Enumerable).
                                GetMethods(BindingFlags.Static | BindingFlags.Public).
                                Where(x => x.Name == "Any").
                                Single(x => x.GetParameters().Length == 1).
                                MakeGenericMethod(contractPropertyType);

            var param = Expression.Parameter(typeof(TQuery), "q");

            return Expression.Lambda<Func<TQuery, bool>>(
                Expression.Call(anyMethod, Expression.Property(param, queryPropertyInfo)),
                param
            );
        }
    }
}
