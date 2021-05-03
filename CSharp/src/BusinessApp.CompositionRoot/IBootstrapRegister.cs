namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Service to register application services
    /// </summary>
    public interface IBootstrapRegister
    {
        void Register(RegistrationContext context);
    }
}

