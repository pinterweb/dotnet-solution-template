namespace BusinessApp.Domain
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq.Expressions;

    /// <summary>
    /// Converts the <see cref="EntityId{T}"/> to the primitive type
    /// </summary>
    public class EntityIdTypeConverter<TId, T> : TypeConverter
        where TId : EntityId<T>
        where T : IComparable
    {
        private static readonly Func<T, TId> Creator;
        private static readonly Func<TId, T> Getter;

        static EntityIdTypeConverter()
        {
            var targetType = typeof(TId);

            var idValueParam = Expression.Parameter(typeof(T), "id");
            var ctor = Expression.New(targetType);
            var idValueProp = targetType.GetProperty("Id");
            var idValAssignment = Expression.Bind(idValueProp, idValueParam);
            var memberInit = Expression.MemberInit(ctor, idValAssignment);

            var getMethodInfo = idValueProp.GetGetMethod();
            var exTarget = Expression.Parameter(targetType, "t");
            var exBody = Expression.Call(exTarget, getMethodInfo);
            var exBody2 = Expression.Convert(exBody, typeof(T));

            Getter = Expression.Lambda<Func<TId, T>>(exBody2, exTarget).Compile();
            Creator = Expression.Lambda<Func<T, TId>>(memberInit, idValueParam).Compile();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => typeof(T) == sourceType;

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            => Creator((T)value);

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => typeof(T) == destinationType;

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            => Getter((TId)value);

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            if (value.GetType() == typeof(T)) return true;

            try
            {
                Convert.ChangeType(value, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
