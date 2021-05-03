using System;
using System.Collections.Generic;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// General type helper methods extending the base class library
    /// </summary>
    public static class TypeExtensions
    {
        public static bool IsGenericIEnumerable(this Type type)
            => type.IsConstructedGenericType
                && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
    }
}
