namespace BusinessApp.App
{
    using System;
    using System.Security;
    using BusinessApp.Domain;

    public class SecurityResourceException : SecurityException
    {
        public SecurityResourceException(string resourceName, string message, Exception inner = null)
            :base(message, inner)
        {
            ResourceName = Guard.Against.Empty(resourceName).Expect(nameof(resourceName));

            Data.Add(ResourceName, Message);
        }

        public string ResourceName { get; }
    }
}
