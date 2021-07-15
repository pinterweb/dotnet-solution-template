using BusinessApp.Infrastructure;
using System;
using System.Linq;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Extension methods to help with registering services
    /// </summary>
    public static partial class RegisterExtensions
    {
        public static bool IsQueryType(this Type type) => typeof(IQuery).IsAssignableFrom(type);

        public static bool IsTypeDefinition(this Type actual, Type test)
            => actual.IsGenericType
                && actual.GetGenericTypeDefinition() == test;
    }
}
