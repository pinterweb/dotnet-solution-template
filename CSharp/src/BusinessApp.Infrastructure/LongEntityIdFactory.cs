using System.ComponentModel;
using BusinessApp.Domain;
using IdGen;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Factory to generate unique ids
    /// </summary>
    public class LongEntityIdFactory<T> : IEntityIdFactory<T> where T : IEntityId
    {
        private static string ErrorMsg = $"{typeof(T).Name} must be able to convert from an Int64";
        private static TypeConverter Converter = TypeDescriptor.GetConverter(typeof(T));
        private static IdGenerator generator = new IdGenerator(0);

        public T Create()
        {
            if (Converter.CanConvertFrom(typeof(long)))
            {
                return (T)Converter.ConvertFrom(generator.CreateId());
            }

            throw new BusinessAppException(ErrorMsg);
        }
    }
}
