namespace BusinessApp.App
{
    using System.Collections;
    using System.Collections.Generic;

    public class EnvelopeContract<TData> : IEnumerable<TData>
    {
        public IEnumerable<TData> Data { get; set; }

        public Pagination Pagination { get; set; }

        public IEnumerator<TData> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
