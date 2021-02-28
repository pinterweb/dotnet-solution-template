namespace BusinessApp.App
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using BusinessApp.Domain;

    /// <summary>
    /// Exception to throw when an entity is not found, but was expected
    /// </summary>
    [Serializable]
    public class BatchException : Exception, IEnumerable<Result<object, Exception>>
    {
        private readonly IEnumerable<Result<object, Exception>> results;

        private BatchException(IEnumerable<Result<object, Exception>> results)
        {
            this.results = results;
        }

        public static BatchException FromResults<T>(IEnumerable<Result<T, Exception>> results)
        {
            results.NotEmpty().Expect(nameof(results));

            var allResults = new List<Result<object, Exception>>();

            foreach (var result in results)
            {
                switch (result.Kind)
                {
                    case ValueKind.Error when result.UnwrapError() is BatchException b:
                        allResults.AddRange(b.Flatten().results);
                        break;
                    default:
                        allResults.Add(result.MapOrElse(
                            e => Result.Error<object>(e),
                            v => Result.Ok<object>(v)
                        ));
                        break;
                };
            }

            return new BatchException(allResults);
        }

        public IEnumerator<Result<object, Exception>> GetEnumerator() => results.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private BatchException Flatten()
        {
            var flattenList = new List<Result<object, Exception>>();

            foreach (var result in results)
            {
                switch (result.Kind)
                {
                    case ValueKind.Error when result.UnwrapError() is BatchException b:
                        flattenList.AddRange(b.Flatten().results);
                        break;
                    default:
                        flattenList.Add(result.MapOrElse(
                            e => Result.Error<object>(e),
                            v => Result.Ok<object>(v)
                        ));
                        break;
                };
            }

            return new BatchException(flattenList);
        }
    }
}
