using BusinessApp.Infrastructure;
using System;
using System.Linq;

namespace BusinessApp.CompositionRoot
{
    public static class RegisterExtensions
    {
        public static bool IsQueryType(this Type type) => typeof(IQuery).IsAssignableFrom(type);

        public static bool IsMacro(this Type type)
            => type
                .GetInterfaces()
                .Any(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IMacro<>));

        public static bool IsTypeDefinition(this Type actual, Type test)
            => actual.IsGenericType
                && actual.GetGenericTypeDefinition() == test;
    }
}
