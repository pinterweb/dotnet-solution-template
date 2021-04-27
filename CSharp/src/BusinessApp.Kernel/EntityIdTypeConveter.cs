using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Converts an <see cref="IEntityId"/> to its primitive type
    /// </summary>
    public class EntityIdTypeConverter<TId, T> : TypeConverter
        where TId : IEntityId
    {
        private static readonly Type innerType = typeof(T);
        private static readonly Type idType = typeof(TId);
        private static readonly TypeConverter inner;
        private static readonly ConstructorInfo? convertFromCtor;

        static EntityIdTypeConverter()
        {
            inner = TypeDescriptor.GetConverter(typeof(T));
            convertFromCtor = idType.GetConstructor(new[] { innerType });
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => convertFromCtor != null
                && (innerType == sourceType || inner.CanConvertFrom(sourceType));

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture,
            object value)
        {
            if (convertFromCtor == null)
            {
                var sourceTypeName = value.GetType();

                throw new FormatException($"To convert from '{sourceTypeName}' to " +
                    $"'{typeof(TId).Name}', the IEntityId needs a constructor that has an '{sourceTypeName}' " +
                    "argument only");
            }

            if (value is T)
            {
                return convertFromCtor.Invoke(new object[] { value });
            }

            var innerValue = inner.ConvertFrom(context, culture, value);

            return convertFromCtor.Invoke(new object[] { innerValue });
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => innerType == destinationType || inner.CanConvertTo(context, destinationType);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
            object value, Type destinationType)
        {
            if (value is not TId)
            {
                throw new FormatException($"Source value must be '{idType.Name}'");
            }

            var innerValue = Convert.ChangeType(value, innerType, CultureInfo.CurrentCulture);

            return innerType == destinationType
                ? innerValue
                : inner.ConvertTo(context, culture, innerValue, destinationType);
        }
    }
}
