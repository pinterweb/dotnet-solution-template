namespace BusinessApp.App
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Security;
    using BusinessApp.Domain;

    public class SecurityResourceException : SecurityException
    {
        public SecurityResourceException(string resourceName, string message, Exception inner = null)
            :base(message, inner)
        {
            ResourceName = GuardAgainst.Empty(resourceName, nameof(resourceName));
        }

        public string ResourceName { get; }

        public override IDictionary Data
        {
            get => new Dictionary<string, string>
            {
                { ResourceName, Message }
            };
        }
    }
}
