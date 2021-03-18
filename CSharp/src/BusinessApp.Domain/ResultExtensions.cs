namespace BusinessApp.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ResultExtensions
    {
        public static Result<IEnumerable<T>, Exception> Collect<T, E>(
            this IEnumerable<Result<T, E>> source)
            where E : Exception
        {
            var errors = source.Where(r => r.Kind == ValueKind.Error);

            if (errors.Count() == 1)
            {
                return Result.Error<IEnumerable<T>>(errors.Single().UnwrapError());
            }
            else if (errors.Any())
            {
                return Result.Error<IEnumerable<T>>(
                    new AggregateException(errors.Select(e => e.UnwrapError())));
            }

            return Result.Ok(source.Select(s => s.Unwrap()));
        }

        public async static Task<Result<R, E>> MapAsync<T, E, R>(
            this Result<T, E> source,
            Func<T, CancellationToken, Task<R>> onOk,
            CancellationToken cancelToken)
        {
            return source.Kind switch
            {
                ValueKind.Ok => Result<R, E>.Ok(await onOk(source.Unwrap(), cancelToken)),
                ValueKind.Error => Result<R, E>.Error(source.UnwrapError()),
                _ => throw new NotImplementedException(),
            };
        }

        public async static Task<Result<R, E>> MapAsync<T, E, R>(
            this Task<Result<T, E>> source, Func<T, R> next)
        {
            var result = await source;

            return result.Map(next);
        }

        public async static Task<Result<T, E>> AndThenAsync<T, E>(
            this Task<Result<T, E>> source,
            Func<T, Result<T, E>> next)
        {
            var result = await source;

            return result.AndThen(next);
        }

        public async static Task<Result<T, E>> AndThenAsync<T, E>(
            this Result<T, E> source,
            Func<T, CancellationToken, Task<Result<T, E>>> next,
            CancellationToken cancelToken)
        {
            return source.Kind switch
            {
                ValueKind.Ok => await next(source.Unwrap(), cancelToken),
                ValueKind.Error => source,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
