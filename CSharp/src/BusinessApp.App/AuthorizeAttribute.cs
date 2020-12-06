namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using BusinessApp.Domain;

    /// <summary>
    /// Specifies that the class or method that this attribute is applied to requires the specified authorization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class.
        /// </summary>
        public AuthorizeAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class with the specified roles.
        /// </summary>
        /// <param name="policy">The roles required for authorization.</param>
        public AuthorizeAttribute(params string[] roles)
        {
            Roles = roles.NotNull().Expect(nameof(roles));
        }

        /// <summary>
        /// Roles that are allowed to access the resource.
        /// </summary>
        public IEnumerable<string> Roles { get; } = new string[0];
    }
}
