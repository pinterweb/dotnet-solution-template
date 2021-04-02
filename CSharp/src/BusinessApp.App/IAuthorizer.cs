namespace BusinessApp.App
{
    /// <summary>
    /// Interface to authorize the execution of the instance
    /// </summary>
    public interface IAuthorizer<T>
        where T : notnull
    {
        void AuthorizeObject(T instance);
    }
}
