using System;
using System.Linq.Expressions;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Linq implementation of the specification pattern
    /// </summary>
    public class LinqSpecification<T> : ISpecification<T>
    {
        public LinqSpecification(Expression<Func<T, bool>> expression)
        {
            Predicate = expression.NotNull().Expect(nameof(expression));
        }

        public virtual Expression<Func<T, bool>> Predicate { get; protected set; }

        public bool IsSatisfiedBy(T value) => Predicate.Compile()(value);

        public static LinqSpecification<T> operator &(LinqSpecification<T> left,
          LinqSpecification<T> right)
        {
            var visitor = new ReplaceExpressionVisitor(left.Predicate.Parameters[0], right.Predicate.Parameters[0]);
            var binaryExpression = Expression.AndAlso(visitor.Visit(left.Predicate.Body), right.Predicate.Body);
            var lambda = Expression.Lambda<Func<T, bool>>(binaryExpression, right.Predicate.Parameters);
            return new LinqSpecification<T>(lambda);
        }

        public static LinqSpecification<T> operator |(LinqSpecification<T> left, LinqSpecification<T> right)
        {
            var visitor = new ReplaceExpressionVisitor(left.Predicate.Parameters[0], right.Predicate.Parameters[0]);
            var binaryExpression = Expression.OrElse(visitor.Visit(left.Predicate.Body), right.Predicate.Body);
            var lambda = Expression.Lambda<Func<T, bool>>(binaryExpression, right.Predicate.Parameters);
            return new LinqSpecification<T>(lambda);
        }

        // Used to make one parameter for two expressions
        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression from, to;

            public ReplaceExpressionVisitor(Expression from, Expression to)
            {
                this.from = from;
                this.to = to;
            }

            public override Expression Visit(Expression? node)
            {
                // XXX node cannot be null because this is private class and we
                // know what we are passing
                return node == from ? to : base.Visit(node)!;
            }
        }
    }
}
