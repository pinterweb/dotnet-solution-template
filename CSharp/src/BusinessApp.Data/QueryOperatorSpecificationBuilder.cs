namespace BusinessApp.Data
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System.Collections.Concurrent;

    public class QueryOperatorSpecificationBuilder<TQuery, TContract> :
        ILinqSpecificationBuilder<TQuery, TContract>
    {
        private static ConcurrentDictionary<Type, ICollection<SpecificationDescriptor>> DescriptorCache
            = new ConcurrentDictionary<Type, ICollection<SpecificationDescriptor>>();

        private static ParameterExpression ContractParam
            = Expression.Parameter(typeof(TContract), "contract");

        private static ParameterExpression QueryParam
            = Expression.Parameter(typeof(TQuery), "query");

        static QueryOperatorSpecificationBuilder()
        {
            DescriptorCache[typeof(TQuery)] = new List<SpecificationDescriptor>();
            CreateDescriptorCache(typeof(TQuery));
        }

        public LinqSpecification<TContract> Build(TQuery query)
        {
            if (DescriptorCache.TryGetValue(query.GetType(), out ICollection<SpecificationDescriptor> e))
            {
                var spec = e.Select(p => CreateSpec(query, p))
                    .Where(s => s != null);

                return spec.Any() ? spec.Aggregate((a, b) => a & b) : new NullSpecification<TContract>(true);
            }

            DescriptorCache[query.GetType()] = new List<SpecificationDescriptor>();
            CreateDescriptorCache(query.GetType());

            return Build(query);
        }

        private static LinqSpecification<TContract> CreateSpec(TQuery query,
            SpecificationDescriptor e)
        {
            var propertyValue = e.PropertyGetter(query);

            if (propertyValue == null) return null;

            var body = GetBody(propertyValue, e);
            var lambda = Expression.Lambda<Func<TContract, bool>>(body, ContractParam);
            return new LinqSpecification<TContract>(lambda);
        }

        private static Expression GetBody(object propVal, SpecificationDescriptor e)
        {
            return MapQueryOperator(e.Attribute, Expression.Constant(propVal), e.ContractProp);
        }

        private static void CreateDescriptorCache(Type queryType)
        {
            var seenTypes = new HashSet<Type>() { queryType };

            void FillCache(PropertyInfo property, Expression queryExp, Expression contractExp)
            {
                var attribute = property.GetCustomAttribute<QueryOperatorAttribute>();

                var queryProp = Expression.Property(
                    Expression.Convert(queryExp, property.DeclaringType), property);

                if (attribute.OperatorToUse == null && !seenTypes.Contains(property.PropertyType))
                {
                    seenTypes.Add(property.PropertyType);

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

                    DescriptorCache[queryType].Add(new SpecificationDescriptor
                    {
                        PropertyGetter =
                           Expression.Lambda<Func<TQuery, object>>(
                               Expression.Condition(
                                   Expression.ReferenceEqual(queryExp, Expression.Constant(null)),
                                   Expression.Constant(null),
                                   Expression.Convert(queryProp, typeof(object))
                               ), QueryParam
                           ).Compile(),
                        ContractProp = contractProp,
                        Attribute = opAttr
                    });
                }
            };

            var props = queryType
                .GetProperties()
                .Where(p => p.IsDefined(typeof(QueryOperatorAttribute)));

            foreach (var p in props) FillCache(p, QueryParam, ContractParam);
        }

        private static Expression MapQueryOperator(
            QueryOperatorAttribute attribute,
            Expression queryMemberExpr,
            MemberExpression contractMemberExpr)
        {
            var contractP = contractMemberExpr.Member as PropertyInfo;
            var queryExp = Nullable.GetUnderlyingType(contractP.PropertyType) switch
            {
                null => (Expression)queryMemberExpr,
                _ => (Expression)Expression.Convert(queryMemberExpr, contractP.PropertyType),
            };

            switch (attribute.OperatorToUse)
            {
                case QueryOperators.Contains:
                    var contractProp = contractMemberExpr.Member as PropertyInfo;
                    var method = typeof(Enumerable)
                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .Where(x => x.Name == "Contains")
                        .Single(x => x.GetParameters().Length == 2)
                        .MakeGenericMethod(contractProp.PropertyType);

                    return Expression.Call(null, method, queryExp, contractMemberExpr);
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
                    throw new BusinessAppDataException($"{attribute.OperatorToUse} is not supported.");
            }
        }

        private sealed class SpecificationDescriptor
        {
            public Func<TQuery, object> PropertyGetter { get; set; }
            public MemberExpression ContractProp { get; set; }
            public QueryOperatorAttribute Attribute { get; set; }
        }
    }
}
