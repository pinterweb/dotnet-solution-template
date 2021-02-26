namespace BusinessApp.WebApi
{
    using BusinessApp.App;
    using System;

    public static class RegisterExtensions
    {
        public static bool IsQueryType(this Type type)
        {
            return typeof(IQuery).IsAssignableFrom(type);
        }
    }
}
