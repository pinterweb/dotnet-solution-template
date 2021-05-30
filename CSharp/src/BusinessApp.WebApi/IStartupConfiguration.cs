namespace BusinessApp.WebApi
{
    /// <summary>
    /// Allows for custom startup configuration to not pollute the Startup file.
    /// Gives a modular approach for different dependencies so they can easily be
    /// swapped in and out.
    /// </summary>
    public interface IStartupConfiguration
    {
        void Configure();
    }
}
