using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Async extensions for the <see cref="Result{T, TErr}" />
    /// </summary>
    public static class AsyncResultExtensions
    {
        /// <summary>
        /// Awaits for the array of results and collects all all results into one <see cref="Result{T,E}" />
        /// </summary>
        public static async Task<Result<IEnumerable<T>, Exception>> CollectAsync<T, TErr>(
            this Task<Result<T, TErr>[]> source)
            where TErr : Exception
        {
            var results = await source;

            return results.Collect();
        }

        /// <summary>
        /// Awaits the result and runs the <see cref="Result{T,E}" /> Map logic
        /// </summary>
        public static async Task<Result<TOut, TErr>> MapAsync<TIn, TErr, TOut>(
            this Task<Result<TIn, TErr>> source, Func<TIn, TOut> op)
        {
            var result = await source;

            return result.Map(op);
        }

        /// <summary>
        /// On success, will await the map function
        /// </summary>
        public static async Task<Result<TOut, TErr>> MapAsync<TIn, TErr, TOut>(
            this Result<TIn, TErr> source, Func<TIn, Task<TOut>> onOk) => source.Kind switch
            {
                ValueKind.Ok => Result<TOut, TErr>.Ok(await onOk(source.Unwrap())),
                ValueKind.Error => Result<TOut, TErr>.Error(source.UnwrapError()),
                _ => throw new NotImplementedException(),
            };

        /// <summary>
        /// Awaits for the result and runs the <see cref="Result.OrElse" /> logic
        /// </summary>
        public static async Task<Result<T, TErr>> OrElseAsync<T, TErr>(
            this Task<Result<T, TErr>> source, Func<TErr, Result<T, TErr>> next)
        {
            var result = await source;

            return result.OrElse(next);
        }

        /// <summary>
        /// Awaits the result and awaits the next task on failure, returning the
        /// original result passed in
        /// </summary>
        /// <remarks>
        /// Can be used to run side effect functions
        /// </summary>
        public static async Task<Result<TIn, TErr>> OrElseRunAsync<TIn, TErr>(
            this Task<Result<TIn, TErr>> source, Func<TErr, Task> next)
        {
            var result = await source;

            switch (result.Kind)
            {
                case ValueKind.Ok:
                    return result;
                case ValueKind.Error:
                    await next(result.UnwrapError());
                    return result;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Awaits the result and runs the next action on failure,
        /// returning the original result passed in
        /// </summary>
        /// <remarks>
        /// Can be used to run side effect functions
        /// </summary>
        public static async Task<Result<TIn, TErr>> OrElseRunAsync<TIn, TErr>(
            this Task<Result<TIn, TErr>> source, Action<TErr> next)
        {
            var result = await source;

            switch (result.Kind)
            {
                case ValueKind.Ok:
                    return result;
                case ValueKind.Error:
                    next(result.UnwrapError());
                    return result;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Awaits for the result and runs AndThen logic
        /// </summary>
        public static async Task<Result<TOut, TErr>> AndThenAsync<TIn, TErr, TOut>(
            this Task<Result<TIn, TErr>> source, Func<TIn, Result<TOut, TErr>> next)
        {
            var result = await source;

            return result.AndThen(next);
        }

        /// <summary>
        /// Awaits for the result and the next function on success returning its <see cref="Result{T, E}" />
        /// </summary>
        public static async Task<Result<TOut, TErr>> AndThenAsync<TIn, TErr, TOut>(
            this Task<Result<TIn, TErr>> source, Func<TIn, Task<Result<TOut, TErr>>> next)
        {
            var result = await source;

            return await result.AndThenAsync(next);
        }

        /// <summary>
        /// On success, awaits the next funvtion and returns its <see cref="Result{T, E}" />
        /// </summary>
        public static async Task<Result<TOut, TErr>> AndThenAsync<TIn, TErr, TOut>(
            this Result<TIn, TErr> source, Func<TIn, Task<Result<TOut, TErr>>> next)
            => source.Kind switch
            {
                ValueKind.Ok => await next(source.Unwrap()),
                ValueKind.Error => Result<TOut, TErr>.Error(source.UnwrapError()),
                _ => throw new NotImplementedException(),
            };

        /// <summary>
        /// Awaits the result and runs the next action/computation on success,
        /// returning the original result passed in
        /// </summary>
        /// <remarks>
        /// Can be used to run side effect functions
        /// </summary>
        public static async Task<Result<TIn, TErr>> AndThenRunAsync<TIn, TErr>(
            this Task<Result<TIn, TErr>> source, Func<TIn, Task> next)
        {
            var result = await source;

            switch (result.Kind)
            {
                case ValueKind.Ok:
                    await next(result.Unwrap());
                    return result;
                case ValueKind.Error:
                    return result;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Awaits for the result and runs the MapOrElse logic
        /// </summary>
        public static async Task<TOut> MapOrElseAsync<TIn, TErr, TOut>(
            this Task<Result<TIn, TErr>> source, Func<TErr, TOut> onError, Func<TIn, TOut> onOk)
        {
            var result = await source;

            return result.MapOrElse(onError, onOk);
        }
    }
}
