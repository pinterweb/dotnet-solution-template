namespace BusinessApp.Kernel
{
    /// <summary>
    /// Null pattern implementation for the specification pattern
    /// </summary>
    public class NullSpecification<T> : LinqSpecification<T>
    {
        public NullSpecification(bool returnType)
            : base(_ => returnType)
        { }
    }
}
