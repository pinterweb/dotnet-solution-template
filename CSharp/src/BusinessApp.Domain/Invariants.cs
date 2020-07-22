namespace BusinessApp.Domain
{
    /// <summary>
    /// Helper classes for enforcing invariants
    /// </summary>
    public static partial class Invariants
    {
        public static Result<T> NotNull<T>(T value) =>
            CreateNotNullSpec<T>().Test(value);

        public static Result<string> NotEmpty(string value) => (
            CreateNotNullSpec<string>()
            & new LinqSpecification<string>(v => v.Trim().Length != 0)
        ).Test(value);

        private static LinqSpecification<T> CreateNotNullSpec<T>() =>
            new LinqSpecification<T>(v => v != null);
    }
}
