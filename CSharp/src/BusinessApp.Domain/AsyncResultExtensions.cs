using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessApp.Domain
{
    public static class AsyncResultExtensions
    {
        public async static Task<Result<IEnumerable<T>, Exception>> CollectAsync<T, E>(
            this Task<Result<T, E>[]> source)
            where E : Exception
        {
            var results = await source;

            return results.Collect();
        }

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
            this Task<Result<T, E>> source, Func<T, R> next)
        {
            var result = await source;

            return result.Map(next);
        }

        public async static Task<Result<R, E>> MapAsync<T, E, R>(
            this Result<T, E> source, Func<T, Task<R>> onOk)
        {
            return source.Kind switch
            {
                ValueKind.Ok => Result<R, E>.Ok(await onOk(source.Unwrap())),
                ValueKind.Error => Result<R, E>.Error(source.UnwrapError()),
                _ => throw new NotImplementedException(),
            };
        }

        public async static Task<Result<T, E>> OrElseAsync<T, E>(
            this Task<Result<T, E>> source, Func<E, Result<T, E>> next)
        {
            var result = await source;

            return result.OrElse(next);
        }

        public async static Task<Result<R, E>> AndThenAsync<T, E, R>(
            this Task<Result<T, E>> source,
            Func<T, Result<R, E>> next)
        {
            var result = await source;

            return result.AndThen(next);
        }

        public async static Task<Result<R, E>> AndThenAsync<T, E, R>(
            this Task<Result<T, E>> source,
            Func<T, Task<Result<R, E>>> next)
        {
            var result = await source;

            return await result.AndThenAsync(next);
        }

        public async static Task<Result<R, E>> AndThenAsync<T, E, R>(
            this Result<T, E> source,
            Func<T, Task<Result<R, E>>> next)
        {
            return source.Kind switch
            {
                ValueKind.Ok => await next(source.Unwrap()),
                ValueKind.Error => Result<R, E>.Error(source.UnwrapError()),
                _ => throw new NotImplementedException(),
            };
        }

        public async static Task<R> MapOrElseAsync<T, E, R>(this Task<Result<T, E>> source,
            Func<E, R> onError, Func<T, R> onOk)
        {
            var result = await source;

            return result.MapOrElse(onError, onOk);
        }
    }
}
