namespace BusinessApp.App
{
    /// <summary>
    /// Implementation of IFileProperties for a rolling log file
    /// </summary>
    public class RollingFileProperties : IFileProperties
    {
        private string name;

        public string Name
        {
            get => string.Format("{0}_{1:yyyyMMdd}.log", name, System.DateTimeOffset.UtcNow);
            set => name = value;
        }
    }
}
