namespace BusinessApp.App
{
    using System;
    using BusinessApp.Domain;

    /// <summary>
    /// Exception to throw when an entity is not found, but was expected
    /// </summary>
    [Serializable]
    public class EntityNotFoundException : BusinessAppException
    {
        public EntityNotFoundException(string message)
            :base(message)
        { }

        public EntityNotFoundException(string entityName, string? message = null)
            :base(message ?? $"{entityName} not found")
        { }
    }
}
