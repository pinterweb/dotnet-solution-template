namespace BusinessApp.Data
{
    using BusinessApp.Domain;

    /// <summary>
    /// Interface to build a <see cref="LinqSpecification{}"/> from the query
    /// </summary>
    public interface ILinqSpecificationBuilder<TQuery, TResult>
    {
        LinqSpecification<TResult> Build(TQuery query);
    }
}
