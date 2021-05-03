using System.Buffers;
using System.IO.Pipelines;

namespace BusinessApp.Infrastructure.WebApi
{
    /// <summary>
    /// Extensions for <see cref="PipeReader" />
    /// </summary>
    public static class PipelineReaderExtensions
    {
        /// <summary>
        /// Goes back to the beginning of the buffer so the data can be read again
        /// </summary>
        public static void RewindTo(this PipeReader reader, ReadOnlySequence<byte> buffer)
            => reader.AdvanceTo(buffer.Start, buffer.Start);
    }
}
