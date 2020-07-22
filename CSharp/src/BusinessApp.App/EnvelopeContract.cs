namespace BusinessApp.App
{
    using System.Collections.Generic;

    public class EnvelopeContract<TContract>
    {
        public IEnumerable<TContract> Data { get; set; }

        public Pagination Pagination { get; set; }
    }
}
