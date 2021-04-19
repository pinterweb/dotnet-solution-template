using System;
using System.Security;
using BusinessApp.Domain;

namespace BusinessApp.App
{
    public class SecurityResourceException : SecurityException
    {
        public SecurityResourceException(string resourceName, string message, Exception? inner = null)
            :base(message, inner)
        {
            ResourceName = resourceName.NotEmpty().Expect(nameof(resourceName));
        }

        public string ResourceName { get; }
    }
}
