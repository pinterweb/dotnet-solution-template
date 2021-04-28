using System.Buffers;
using System.IO.Pipelines;

namespace BusinessApp.Infrastructure.WebApi
{
    public static class PipelineReaderExtensions
    {
        public static void RewindTo(this PipeReader reader, ReadOnlySequence<byte> buffer)
            => reader.AdvanceTo(buffer.Start, buffer.Start);
    }
}
