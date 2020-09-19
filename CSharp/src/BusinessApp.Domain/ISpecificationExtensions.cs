namespace BusinessApp.Domain
{
    public static class ISpecificationExtensions
    {
        /// <summary>
        /// Converts the specification result into a <see cref="Result{T}"/>
        /// that can be further acted on
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spec"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Result<T> Test<T>(this ISpecification<T> spec, T value)
        {
            if (!spec.IsSatisfiedBy(value)) return Result<T>.Error(value);

            return Result<T>.Ok(value);
        }

        public static bool Test<TSpec, T>(T value)
            where TSpec : ISpecification<T>, new()
        {
            var spec = new TSpec();

            return spec.IsSatisfiedBy(value);
        }
    }
}
