using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BusinessApp.Kernel;
using System.Collections.Concurrent;

namespace BusinessApp.Infrastructure
{
    public class QueryOperatorSpecificationBuilder<TQuery, TContract> :
        ILinqSpecificationBuilder<TQuery, TContract>
        where TQuery : notnull
    {
        private static readonly ConcurrentDictionary<Type, ICollection<SpecificationDescriptor>> descriptorCache
            = new();

        private static readonly ParameterExpression contractParam
            = Expression.Parameter(typeof(TContract), "contract");

        private static readonly ParameterExpression queryParam
            = Expression.Parameter(typeof(TQuery), "query");

        static QueryOperatorSpecificationBuilder()
        {
            descriptorCache[typeof(TQuery)] = new List<SpecificationDescriptor>();
            CreateDescriptorCache(typeof(TQuery));
        }

        public LinqSpecification<TContract> Build(TQuery query)
        {
            if (descriptorCache.TryGetValue(query.GetType(), out var e))
            {
#pragma warning disable IDE0007
                IEnumerable<LinqSpecification<TContract>> spec = e
                    .Select(p => CreateSpec(query, p))
                    .Where(s => s != null)!;
#pragma warning restore IDE0007

                return spec.Any() ? spec.Aggregate((a, b) => a & b) : new NullSpecification<TContract>(true);
            }

            descriptorCache[query.GetType()] = new List<SpecificationDescriptor>();
            CreateDescriptorCache(query.GetType());

            return Build(query);
        }

        private static LinqSpecification<TContract>? CreateSpec(TQuery query,
            SpecificationDescriptor e)
        {
            var propertyValue = e.PropertyGetter(query);

            if (propertyValue == null) return null;

            var body = GetBody(propertyValue, e);
            var lambda = Expression.Lambda<Func<TContract, bool>>(body, contractParam);
            return new LinqSpecification<TContract>(lambda);
        }

        private static Expression GetBody(object propVal, SpecificationDescriptor e)
            => MapQueryOperator(e.Attribute, Expression.Constant(propVal), e.ContractProp);

        private static void CreateDescriptorCache(Type queryType)
        {
            var seenTypes = new HashSet<Type>() { queryType };

            void FillCache(PropertyInfo property, Expression queryExp, Expression contractExp)
            {
                var attribute = property.GetCustomAttribute<QueryOperatorAttribute>()!;

                var queryProp = Expression.Property(
                    Expression.Convert(queryExp, property.DeclaringType!), property);

                if (attribute.OperatorToUse == null && !seenTypes.Contains(property.PropertyType))
                {
                    _ = seenTypes.Add(property.PropertyType);

                    var props = property.PropertyType
                        .GetProperties()
                        .Where(p => p.IsDefined(typeof(QueryOperatorAttribute)));

                    foreach (var p in props)
                    {
                        var contractProp = Expression.Property(contractExp, attribute.TargetProp);
                        FillCache(p,
                               Expression.Condition(
                                   Expression.ReferenceEqual(queryExp, Expression.Constant(null)),
                                   Expression.Constant(null),
                                   Expression.Convert(queryProp, typeof(object))
                               ),
                            contractProp);
                    }
                }
                else if (attribute is QueryOperatorAttribute opAttr)
                {
                    var contractProp = Expression.Property(contractExp, opAttr.TargetProp);

                    descriptorCache[queryType].Add(new SpecificationDescriptor(
                           Expression.Lambda<Func<TQuery, object>>(
                               Expression.Condition(
                                   Expression.ReferenceEqual(queryExp, Expression.Constant(null)),
                                   Expression.Constant(null),
                                   Expression.Convert(queryProp, typeof(object))
                               ), queryParam
                           ).Compile(),
                        contractProp,
                        opAttr
                    ));
                }
            };

            var props = queryType
                .GetProperties()
                .Where(p => p.DeclaringType != null && p.IsDefined(typeof(QueryOperatorAttribute)));

            foreach (var p in props) FillCache(p, queryParam, contractParam);
        }

        private static Expression MapQueryOperator(
            QueryOperatorAttribute attribute,
            Expression queryMemberExpr,
            MemberExpression contractMemberExpr)
        {
            var contractProp = contractMemberExpr.Member as PropertyInfo;
            var needToConvert = Nullable.GetUnderlyingType(contractProp!.PropertyType) != null
                && attribute.OperatorToUse != QueryOperators.Contains;

#pragma warning disable IDE0072
            var queryExp = needToConvert switch
            {
                false => queryMemberExpr,
                true => Expression.Convert(queryMemberExpr, contractProp.PropertyType),
            };
#pragma warning restore IDE0072

            switch (attribute.OperatorToUse)
            {
                case QueryOperators.Contains:
                    var method = typeof(Enumerable)
                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .Where(x => x.Name == "Contains")
                        .Single(x => x.GetParameters().Length == 2)
                        .MakeGenericMethod(contractProp.PropertyType);

                    return Expression.Call(null, method, queryMemberExpr, contractMemberExpr);
                case QueryOperators.GreaterThanOrEqualTo:
                    return Expression.GreaterThanOrEqual(contractMemberExpr, queryExp);
                case QueryOperators.LessThanOrEqualTo:
                    return Expression.LessThanOrEqual(contractMemberExpr, queryExp);
                case QueryOperators.GreaterThan:
                    return Expression.GreaterThan(contractMemberExpr, queryExp);
                case QueryOperators.LessThan:
                    return Expression.LessThan(contractMemberExpr, queryExp);
                case QueryOperators.Equal:
                    return Expression.Equal(contractMemberExpr, queryExp);
                case QueryOperators.NotEqual:
                    return Expression.NotEqual(contractMemberExpr, queryExp);
                case QueryOperators.StartsWith:
                    var startsWith = typeof(string)
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.Name == "StartsWith")
                        .Single(x =>
                            x.GetParameters().Length == 1 &&
                            x.GetParameters().First().ParameterType == typeof(string));

                    return Expression.Call(contractMemberExpr, startsWith, queryExp);
                default:
                    throw new BusinessAppException($"{attribute.OperatorToUse} is not supported.");
            }
        }

        private sealed class SpecificationDescriptor
        {
            public SpecificationDescriptor(Func<TQuery, object> propertyGetter,
                MemberExpression contractProp,
                QueryOperatorAttribute attribute)
            {
                PropertyGetter = propertyGetter;
                ContractProp = contractProp;
                Attribute = attribute;
            }

            public Func<TQuery, object> PropertyGetter { get; }
            public MemberExpression ContractProp { get; }
            public QueryOperatorAttribute Attribute { get; }
        }
    }
}
