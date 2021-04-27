using System.ComponentModel;
using BusinessApp.Kernel;
using IdGen;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Factory to generate unique ids
    /// </summary>
    public class LongEntityIdFactory<T> : IEntityIdFactory<T> where T : IEntityId
    {
        private static readonly string errorMsg = $"{typeof(T).Name} must be able to convert from an Int64";
        private static readonly TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
        private static readonly IdGenerator generator = new(0);

        public T Create() => converter.CanConvertFrom(typeof(long))
            ? (T)converter.ConvertFrom(generator.CreateId())
            : throw new BusinessAppException(errorMsg);
    }
}
