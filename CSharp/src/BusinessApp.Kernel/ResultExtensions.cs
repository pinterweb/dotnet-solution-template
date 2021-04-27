using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessApp.Kernel
{
    public static class ResultExtensions
    {
        public static Result<IEnumerable<T>, Exception> Collect<T, TErr>(
            this IEnumerable<Result<T, TErr>> source) where TErr : Exception
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
    }
}
