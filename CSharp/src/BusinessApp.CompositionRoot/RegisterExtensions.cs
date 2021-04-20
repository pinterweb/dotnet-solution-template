using BusinessApp.Infrastructure;
using System;
using System.Linq;

namespace BusinessApp.CompositionRoot
{
    public static class RegisterExtensions
    {
        public static bool IsQueryType(this Type type)
        {
            return typeof(IQuery).IsAssignableFrom(type);
        }

        public static bool IsMacro(this Type type)
        {
            return type
                .GetInterfaces()
                .Any(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IMacro<>));
        }
    }
}
