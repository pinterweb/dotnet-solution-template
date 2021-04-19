using BusinessApp.Domain;

namespace BusinessApp.Data
{
    /// <summary>
    /// Interface to build a <see cref="LinqSpecification{}"/> from the query
    /// </summary>
    public interface ILinqSpecificationBuilder<in TQuery, TResult>
        where TQuery : notnull
    {
        LinqSpecification<TResult> Build(TQuery query);
    }
}
