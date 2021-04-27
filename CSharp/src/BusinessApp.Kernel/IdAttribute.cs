using System;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Attribute class to identifier properties that make up the identity of the entity or
    /// value object
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = false, Inherited = true)]
    public class IdAttribute : Attribute
    { }
}
