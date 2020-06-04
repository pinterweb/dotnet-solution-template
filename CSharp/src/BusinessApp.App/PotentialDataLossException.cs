namespace BusinessApp.App
{
    using System;
    using BusinessApp.Domain;

    [Serializable]
    public class PotentialDataLossException : Exception
    {
        public PotentialDataLossException(string message)
            : this(message, null)
        { }

        public PotentialDataLossException(string message, Exception innerException)
            : base(message, innerException)
        {
            GuardAgainst.Empty(message, nameof(message));

            Data.Add("", message);
        }
    }
}
