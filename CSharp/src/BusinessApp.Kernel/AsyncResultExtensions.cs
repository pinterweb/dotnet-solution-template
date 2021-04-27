using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessApp.Kernel
{
    public static class AsyncResultExtensions
    {
        public static async Task<Result<IEnumerable<T>, Exception>> CollectAsync<T, TErr>(
            this Task<Result<T, TErr>[]> source)
            where TErr : Exception
        {
            var results = await source;

            return results.Collect();
        }

        public static async Task<Result<TOut, TErr>> MapAsync<TIn, TErr, TOut>(
            this Task<Result<TIn, TErr>> source, Func<TIn, TOut> op)
        {
            var result = await source;

            return result.Map(op);
        }

        public static async Task<Result<TOut, TErr>> MapAsync<TIn, TErr, TOut>(
            this Result<TIn, TErr> source, Func<TIn, Task<TOut>> onOk) => source.Kind switch
            {
                ValueKind.Ok => Result<TOut, TErr>.Ok(await onOk(source.Unwrap())),
                ValueKind.Error => Result<TOut, TErr>.Error(source.UnwrapError()),
                _ => throw new NotImplementedException(),
            };

        public static async Task<Result<T, TErr>> OrElseAsync<T, TErr>(
            this Task<Result<T, TErr>> source, Func<TErr, Result<T, TErr>> next)
        {
            var result = await source;

            return result.OrElse(next);
        }

        public static async Task<Result<TOut, TErr>> AndThenAsync<TIn, TErr, TOut>(
            this Task<Result<TIn, TErr>> source, Func<TIn, Result<TOut, TErr>> next)
        {
            var result = await source;

            return result.AndThen(next);
        }

        public static async Task<Result<TOut, TErr>> AndThenAsync<TIn, TErr, TOut>(
            this Task<Result<TIn, TErr>> source, Func<TIn, Task<Result<TOut, TErr>>> next)
        {
            var result = await source;

            return await result.AndThenAsync(next);
        }

        public static async Task<Result<TOut, TErr>> AndThenAsync<TIn, TErr, TOut>(
            this Result<TIn, TErr> source, Func<TIn, Task<Result<TOut, TErr>>> next)
            => source.Kind switch
            {
                ValueKind.Ok => await next(source.Unwrap()),
                ValueKind.Error => Result<TOut, TErr>.Error(source.UnwrapError()),
                _ => throw new NotImplementedException(),
            };

        public static async Task<TOut> MapOrElseAsync<TIn, TErr, TOut>(
            this Task<Result<TIn, TErr>> source, Func<TErr, TOut> onError, Func<TIn, TOut> onOk)
        {
            var result = await source;

            return result.MapOrElse(onError, onOk);
        }
    }
}
