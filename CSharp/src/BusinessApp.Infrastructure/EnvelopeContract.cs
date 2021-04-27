using System.Collections;
using System.Collections.Generic;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    public class EnvelopeContract<TData> : IEnumerable<TData>
    {
        public EnvelopeContract(IEnumerable<TData> data, Pagination pagination)
        {
            Data = data.NotNull().Expect(nameof(data));
            Pagination = pagination.NotNull().Expect(nameof(data));
        }

        public IEnumerable<TData> Data { get; }
        public Pagination Pagination { get; }
        public IEnumerator<TData> GetEnumerator() => Data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
