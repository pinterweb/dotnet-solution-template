namespace BusinessApp.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Helper classes for enforcing invariants
    /// </summary>
    public static class Invariants
    {
        public static Result<T> NotNull<T>(T value) =>
            CreateNotNullSpec<T>().Test(value);

        public static Result<T> NotEmpty<T>(T value) => (
            CreateNotNullSpec<T>()
            & new LinqSpecification<T>(v => v.ToString().Trim().Length != 0)
        ).Test(value);

        public static Result<T> NotDefault<T>(T value) => (
            new LinqSpecification<T>(v => !EqualityComparer<T>.Default.Equals(v, default(T)))
        ).Test(value);

        private static LinqSpecification<T> CreateNotNullSpec<T>() =>
            new LinqSpecification<T>(v => v != null);
    }
}
