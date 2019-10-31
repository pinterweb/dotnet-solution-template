namespace BusinessApp.App
{
    /// <summary>
    /// Interface to authorize the execution of the instance
    /// </summary>
    public interface IAuthorizer<T>
    {
        void AuthorizeObject(T instance);
    }
}
