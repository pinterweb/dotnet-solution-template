namespace BusinessApp.Data.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    class FakeQueryable<T> : IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly IEnumerable<T> resultSet;

        public FakeQueryable(IEnumerable<T> resultSet)
        {
            this.resultSet = resultSet;
        }

        public Type ElementType => throw new NotImplementedException();

        public Expression Expression => throw new NotImplementedException();

        public IQueryProvider Provider => throw new NotImplementedException();

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new FakeEnumerator(resultSet);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new FakeEnumerator(resultSet);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private class FakeEnumerator : IAsyncEnumerator<T>, IEnumerator<T>
        {
            readonly IEnumerator<T> inner;

            public FakeEnumerator(IEnumerable<T> resultSet)
            {
                inner = resultSet.GetEnumerator();
            }

            public T Current => inner.Current;

            object IEnumerator.Current => inner.Current;

            public void Dispose()
            {
                inner.Dispose();
            }

            public ValueTask DisposeAsync()
            {
                Dispose();
                return new ValueTask();
            }

            public bool MoveNext()
            {
                return inner.MoveNext();
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(MoveNext());
            }

            public void Reset() => inner.Reset();
        }
    }
}
