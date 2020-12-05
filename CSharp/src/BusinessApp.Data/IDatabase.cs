namespace BusinessApp.Data
{
    public interface IDatabase
    {
        void AddOrReplace<TEntity>(TEntity entity) where TEntity : class;
        void Remove<TEntity>(TEntity entity) where TEntity : class;
    }
}
