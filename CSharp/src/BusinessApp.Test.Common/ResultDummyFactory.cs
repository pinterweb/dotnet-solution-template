namespace BusinessApp.Test
{
    using System;
    using BusinessApp.Domain;
    using FakeItEasy;

    public class ResultDummyFactory<T, E> : DummyFactory<Result<T, E>>
        where E : IFormattable
    {
        protected override Result<T, E> Create()
        {
            return Result<T, E>.Ok(default);
        }
    }
}
