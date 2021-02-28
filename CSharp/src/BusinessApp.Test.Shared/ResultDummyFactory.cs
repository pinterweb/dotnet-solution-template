namespace BusinessApp.Test.Shared
{
    using BusinessApp.Domain;
    using FakeItEasy;

    public class ResultDummyFactory<T, E> : DummyFactory<Result<T, E>>
    {
        protected override Result<T, E> Create()
        {
            return Result<T, E>.Ok(default);
        }
    }
}
