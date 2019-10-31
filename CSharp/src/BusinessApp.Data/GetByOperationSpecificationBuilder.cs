namespace BusinessApp.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BusinessApp.App;
    using BusinessApp.Domain;

    /// <summary>
    /// Builds a specification that can handle the data in <see cref="IOperationQuery{TValue}"/>
    /// </summary>
    public abstract class GetByOperationSecificationBuilder<TQuery, TContract, TValue> :
        ILinqSpecificationBuilder<TQuery, TContract>
        where TQuery : IOperationQuery<TValue>
    {
        protected abstract LinqSpecification<TContract> GreaterThanOrEqualTo(TValue val);
        protected abstract LinqSpecification<TContract> GreaterThan(TValue val);
        protected abstract LinqSpecification<TContract> LessThanOrEqualTo(TValue val);
        protected abstract LinqSpecification<TContract> LessThan(TValue val);
        protected abstract LinqSpecification<TContract> Between(TValue min, TValue max);
        protected abstract LinqSpecification<TContract> NotBetween(TValue min, TValue max);
        protected abstract LinqSpecification<TContract> Contains(IEnumerable<TValue> vals);
        protected abstract LinqSpecification<TContract> Equals(TValue val);

        public LinqSpecification<TContract> Build(TQuery query)
        {
            if (query == null || query.Values == null || !query.Values.Any())
            {
                return new NullSpecification<TContract>(true);
            }

            var sizeCount = query.Values.Count();

            if (sizeCount == 1)
            {
                var size = query.Values.First();

                switch (query.Operator)
                {
                    case Operator.GreaterThanOrEqualTo:
                        return GreaterThanOrEqualTo(size);
                    case Operator.GreaterThan:
                        return GreaterThan(size);
                    case Operator.LessThanOrEqualTo:
                        return LessThanOrEqualTo(size);
                    case Operator.LessThan:
                        return LessThan(size);
                    default:
                        return Equals(size);
                }
            }

            if (sizeCount > 2)
            {
                return Contains(query.Values);
            }

            var min = query.Values.Min();
            var max = query.Values.Max();

            switch (query.Operator)
            {
                case Operator.Exclusion:
                    return NotBetween(min, max);
                case Operator.Inclusion:
                    return Between(min, max);
                default:
                    throw new NotSupportedException($"The {query.Operator} operator needs only one size");
            }
        }
    }
}
