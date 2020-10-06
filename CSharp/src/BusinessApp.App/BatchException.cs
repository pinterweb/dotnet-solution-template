namespace BusinessApp.App
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using BusinessApp.Domain;

    /// <summary>
    /// Exception to throw when an entity is not found, but was expected
    /// </summary>
    [Serializable]
    public class BatchException : Exception, IFormattable, IEnumerable<Result>,
        IEnumerable<Result<IFormattable, IFormattable>>
    {
        public BatchException(IEnumerable<Result> results)
        {
            Guard.Against.Empty(results).Expect(nameof(results)).ToList();

            var allResults = new List<Result>();

            foreach (var result in results)
            {
                switch (result.Kind)
                {
                    case ValueKind.Error when result.Into().UnwrapError() is BatchException b:
                        allResults.AddRange(b.Flatten().Results);
                        break;
                    default:
                        allResults.Add(result);
                        break;
                };
            }

            Results = allResults;
        }

        public IReadOnlyCollection<Result> Results { get; }

        public IEnumerator<Result> GetEnumerator() => Results.GetEnumerator();

        IEnumerator<Result<IFormattable, IFormattable>> IEnumerable<Result<IFormattable, IFormattable>>.GetEnumerator()
            => Results.Select(r => r.Into()).GetEnumerator();

        public override string ToString() => ToString("G", null);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

            var errors = Results.Where(r => r.Kind == ValueKind.Error).ToList();

            return string.Format(
                formatProvider,
                "The batch request has {0} out of {1} errors",
                errors.Count, Results.Count);
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private BatchException Flatten()
        {
            var results = new List<Result>();

            foreach (var result in Results)
            {
                switch (result.Kind)
                {
                    case ValueKind.Error when result.Into().UnwrapError() is BatchException b:
                        results.AddRange(b.Flatten().Results);
                        break;
                    default:
                        results.Add(result);
                        break;
                };
            }

            return new BatchException(results);
        }
    }
}
