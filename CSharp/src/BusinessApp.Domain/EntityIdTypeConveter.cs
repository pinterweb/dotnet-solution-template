namespace BusinessApp.Domain
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Converts an <see cref="IEntityId"/> to its primitive type
    /// </summary>
    public class EntityIdTypeConverter<TId, T> : TypeConverter
        where TId : IEntityId
    {
        private static readonly Type InnerType = typeof(T);
        private static readonly Type IdType = typeof(TId);
        private static readonly TypeConverter Inner;
        private static readonly ConstructorInfo ConvertFromCtor;

        static EntityIdTypeConverter()
        {
            Inner = TypeDescriptor.GetConverter(typeof(T));
            ConvertFromCtor = IdType.GetConstructor(new[] { InnerType });
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (ConvertFromCtor == null) return false;

            return InnerType == sourceType || Inner.CanConvertFrom(sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture,
            object value)
        {
            if (ConvertFromCtor == null)
            {
                var sourceTypeName = value.GetType();
                throw new BusinessAppException($"To convert from '{sourceTypeName}' to " +
                    $"'{typeof(TId).Name}', the IEntityId needs a constructor that has an '{sourceTypeName}' " +
                    "argument only");
            }

            if (value is T)
            {
                return ConvertFromCtor.Invoke(new object[] { value });
            }

            var innerValue = Inner.ConvertFrom(context, culture, value);

            return ConvertFromCtor.Invoke(new object[] { innerValue });
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => InnerType == destinationType || Inner.CanConvertTo(context, destinationType);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
            object value, Type destinationType)
        {
            if (!(value is TId))
            {
                throw new BusinessAppException($"Source value must be '{IdType.Name}'");
            }

            var innerValue = Convert.ChangeType(value, InnerType);

            if (InnerType == destinationType) return innerValue;

            return Inner.ConvertTo(context, culture, innerValue, destinationType);
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            try
            {
                var valueType = value?.GetType();

                return CanConvertFrom(context, valueType);
            }
            catch
            {
                return false;
            }
        }
    }
}
