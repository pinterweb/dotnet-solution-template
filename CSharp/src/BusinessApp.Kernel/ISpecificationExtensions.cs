namespace BusinessApp.Kernel
{
    public static class ISpecificationExtensions
    {
        public static bool Test<TSpec, T>(T value)
            where TSpec : ISpecification<T>, new()
        {
            var spec = new TSpec();

            return spec.IsSatisfiedBy(value);
        }
    }
}
