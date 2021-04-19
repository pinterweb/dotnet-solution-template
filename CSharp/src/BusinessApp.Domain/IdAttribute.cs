using System;

namespace BusinessApp.Domain
{
    /// <summary>
    /// Attribute class to identifier properties that make up the identity of the entity or
    /// value object
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property,
    AllowMultiple = false, Inherited = true)]
    public class IdAttribute : Attribute
    { }
}
