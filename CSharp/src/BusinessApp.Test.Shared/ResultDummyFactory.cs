using BusinessApp.Domain;
using FakeItEasy;

namespace BusinessApp.Test.Shared
{
    public class ResultDummyFactory<T, E> : DummyFactory<Result<T, E>>
    {
        protected override Result<T, E> Create()
        {
            return Result<T, E>.Ok(default);
        }
    }
}
