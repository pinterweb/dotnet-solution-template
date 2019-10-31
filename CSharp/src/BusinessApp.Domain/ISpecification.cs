namespace BusinessApp.Domain
{
    /// <summary>
    /// Interface for the specification pattern
    /// </summary>
    public interface ISpecification<in TValue>
    {
        bool IsSatisfiedBy(TValue value);
    }
}
