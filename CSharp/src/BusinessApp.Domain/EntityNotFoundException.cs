namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// Exception to throw when an entity is not found, but was expected
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, string message = null)
            :base(message ?? $"{entityName} not found")
        {  }
    }
}
