namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// Exception to throw when an entity is not found, but was expected
    /// </summary>
    [Serializable]
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message)
            :base(message)
        {
            Data.Add("", message);
        }

        public EntityNotFoundException(string entityName, string message = null)
            :base(message ?? $"{entityName} not found")
        {
            Data.Add(entityName, message);
        }
    }
}
