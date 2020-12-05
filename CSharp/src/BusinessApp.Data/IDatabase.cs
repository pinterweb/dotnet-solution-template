namespace BusinessApp.Data
{
    public interface IDatabase
    {
        void AddOrReplace<TEntity>(TEntity entity);
        void Remove<TEntity>(TEntity entity);
    }
}
