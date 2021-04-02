namespace BusinessApp.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Attribute class to identifier properties that make up the identity of the entity or
    /// value object
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property,
    AllowMultiple = false, Inherited = true)]
    public class KeyIdAttribute : Attribute
    { }

    public class KeyComparer<T> : IEqualityComparer<T>
    {
        private static ExpressionVisitor[] Visitors =
        {
            new CaseInsensitiveExpressionVisitor()
        };
        private static ICollection<PropertyInfo> KeyProperties;
        private static EqualityFunctions Functions;

        static KeyComparer()
        {
            KeyProperties = typeof(T)
                .GetProperties()
                .Where(p => p.IsDefined(typeof(KeyIdAttribute)))
                .ToList();

            Functions = new EqualityFunctions(MakeEqualsMethod(), MakeGetHashCodeMethod());
        }

        public bool Equals(T? x, T? y)
        {
            if (x is null && y is null) return true;
            if (x is null) return y!.Equals(x);
            if (y is null) return x.Equals(y);

            if (KeyProperties.Count == 0)
            {
                x.Equals(y);
            }

            return Functions.EqualsFunc(x, y);
        }

        public int GetHashCode(T obj)
        {
            unchecked
            {
                return KeyProperties.Count == 0
                    ? (obj?.GetHashCode() ?? 0)
                    : Functions.GetHashCodeFunc(this);
            }
        }

        private static Func<object, object, bool> MakeEqualsMethod()
        {
            ParameterExpression a = Expression.Parameter(typeof(object), "a");
            ParameterExpression b = Expression.Parameter(typeof(object), "b");

            UnaryExpression castA = Expression.Convert(a, typeof(T));
            UnaryExpression castB = Expression.Convert(b, typeof(T));

            Expression CreatePopertyEqualsCall(PropertyInfo property)
            {
                Expression propa = Expression.Property(castA, property);
                Expression propb = Expression.Property(castB, property);

                foreach (var v in Visitors)
                {
                    propa = v.Visit(propa);
                    propb = v.Visit(propb);
                }

                return Expression.Call(
                  typeof(object).GetMethod("Equals", BindingFlags.Public | BindingFlags.Static)!,
                  Expression.Convert(propa, typeof(object)),
                  Expression.Convert(propb, typeof(object)));
            };

            var equals = KeyProperties
                .Select(p => CreatePopertyEqualsCall(p))
                .Aggregate((a, b) => Expression.AndAlso(a, b));

            var func = Expression.Condition(Expression.AndAlso(
              Expression.ReferenceNotEqual(b, Expression.Constant(null)),
              Expression.TypeIs(b, typeof(T))),
              Expression.OrElse(Expression.ReferenceEqual(a, b), equals),
              Expression.Constant(false, typeof(bool)));

            return Expression.Lambda<Func<object, object, bool>>(func, a, b).Compile();
        }

        private static Func<object, int> MakeGetHashCodeMethod()
        {
            ParameterExpression a = Expression.Parameter(typeof(object), "x");
            UnaryExpression castA = Expression.Convert(a, typeof(T));

            var hashMultiplier = Expression.Constant(31, typeof(int));
            Expression hashBase = Expression.Constant(17, typeof(int));

            Expression CreateHashCalls(PropertyInfo property)
            {
                Expression member = Expression.Property(castA, property);

                foreach (var v in Visitors)
                {
                    member = v.Visit(member);
                }

                return Expression.Condition(
                    Expression.ReferenceNotEqual(a, Expression.Constant(null)),
                    Expression.Call(
                        Expression.Convert(member, typeof(object)),
                        "GetHashCode",
                        Type.EmptyTypes
                    ),
                    Expression.Constant(0, typeof(int)));
            }

            var hashExpression = KeyProperties
                .Select(p => CreateHashCalls(p))
                .Aggregate(
                    hashBase,
                    (a, b) => Expression.Add(Expression.Multiply(a, hashMultiplier), b));

            return Expression.Lambda<Func<object, int>>(hashExpression, a).Compile();
        }

        private sealed class EqualityFunctions
        {
            public EqualityFunctions(Func<object, object, bool> equalsFn, Func<object, int> hashFn)
            {
                EqualsFunc = equalsFn;
                GetHashCodeFunc = hashFn;
            }

            public Func<object, object, bool> EqualsFunc { get; }
            public Func<object, int> GetHashCodeFunc { get; }
        }

        private sealed class CaseInsensitiveExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Type == typeof(String))
                {
                    var methodInfo = typeof(String).GetMethod("ToLower", new Type[] { })!;
                    var expression = Expression.Condition(
                        Expression.NotEqual(
                            node,
                            Expression.Constant(null)
                        ),
                        Expression.Call(node, methodInfo),
                        node);
                    return expression;
                }

                return base.VisitMember(node);
            }
        }
    }
}
