namespace BusinessApp.App
{
    using System;

    /// <summary>
    /// Interface create a new scope of dependency creations. Any instance created within the
    /// scope should be isolated
    /// </summary>
    public interface IAppScope
    {
        IDisposable NewScope();
    }
}
