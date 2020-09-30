namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// Indicator for success/failure
    /// </summary>
    public enum Result
    {
        Ok,
        Error
    }

    /// <summary>
    /// A value type for an "ignored" type. Null pattern for a throw away type/value
    /// </summary>
    public struct _ : IFormattable
    {
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return "";
        }
    }

    /// <summary>
    /// Wraps a value with the outcome, normally from enforcing invariants
    /// and provides access to it or throws an exception
    /// </summary>
    public struct Result<T, E> where E : IFormattable
    {
        private readonly T value;
        private readonly E error;

        public static Result<T, E> Ok(T value) => new Result<T, E>(value, default, Result.Ok);
        public static Result<T, E> Error(E error) => new Result<T, E>(default, error, Result.Error);

        private Result(T value = default, E error = default, Result result = default)
        {
            this.value = value;
            this.error = error;
            this.Kind = result;
        }

        public Result Kind { get; }

        public T Expect(string message)
        {
            return Kind switch
            {
                Result.Ok => value,
                Result.Error => throw new BadStateException($"{message}: {error}"),
                    _ => throw new NotImplementedException(),
            };
        }

        public T Expect<TException>(string message) where TException : Exception
        {
            try
            {
                return Kind switch
                {
                    Result.Ok => value,
                    Result.Error =>
                        throw (TException)Activator.CreateInstance(typeof(TException), $"{message}: {error}"),
                    _ => throw new NotImplementedException(),
                };
            }
            catch (MissingMethodException)
            {
                throw new BadStateException(
                    $"'{typeof(TException).Name}' does not have a constructor with string parameter. " +
                    $"Original error value is: '{error}'");
            }
        }

        public E ExpectError(string message)
        {
            return Kind switch
            {
                Result.Ok => throw new BadStateException($"{message}: {value}"),
                Result.Error => error,
                _ => throw new NotImplementedException(),
            };
        }

        public E UnwrapError()
        {
            return Kind switch
            {
                Result.Ok => throw new BadStateException($"{value}"),
                Result.Error => error,
                _ => throw new NotImplementedException(),
            };
        }

        public T Unwrap()
        {
            return Kind switch
            {
                Result.Ok => value,
                Result.Error => throw new BadStateException($"{error}"),
                _ => throw new NotImplementedException(),
            };
        }

        public Result<T, E> AndThen(Func<T, Result<T, E>> next)
        {
            return Kind switch
            {
                Result.Ok => next(value),
                Result.Error => this,
                _ => throw new NotImplementedException(),
            };
        }

        public Result<T, E> OrElse(Func<E, Result<T, E>> onError)
        {
            return Kind switch
            {
                Result.Error => onError(error),
                Result.Ok => this,
                _ => throw new NotImplementedException(),
            };
        }

        public Result<R, E> Map<R>(Func<T, R> onOk)
        {
            return Kind switch
            {
                Result.Error => Result<R, E>.Error(error),
                Result.Ok => Result<R, E>.Ok(onOk(value)),
                _ => throw new NotImplementedException(),
            };
        }

        public R MapOrElse<R>(Func<E, R> onError, Func<T, R> onOk)
        {
            return Kind switch
            {
                Result.Error => onError(error),
                Result.Ok => onOk(value),
                _ => throw new NotImplementedException(),
            };
        }

        public static implicit operator T(Result<T, E> result)
        {
            return result.Expect("Cannot implictly get the value because it is an error");
        }

        public static implicit operator E(Result<T, E> result)
        {
            return result.ExpectError("Cannot implictly get the error because it is a valid value");
        }

        public static implicit operator Result(Result<T, E> result) => result.Kind;

        public static implicit operator bool(Result<T, E> result) => result.Kind == Result.Ok;
    }
}
