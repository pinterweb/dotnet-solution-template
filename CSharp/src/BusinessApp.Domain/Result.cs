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
    /// A value type for an "ignored" type
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
            switch (Kind)
            {
                case Result.Ok:
                    return value;
                default:
                    throw new BadStateException($"{message}: {error}");
            }
        }

        public T Expect<TException>(string message) where TException : Exception
        {
            try
            {
                throw (TException)Activator.CreateInstance(typeof(TException), $"{message}: {error}");
            }
            catch (MissingMethodException)
            {
                throw new BadStateException(
                    $"'{typeof(TException).Name}' does not have a constructor with string parameter. " +
                    $"Original error value is: '{error}'");
            }
        }

        public T Unwrap()
        {
            switch (Kind)
            {
                case Result.Ok:
                    return value;
                default:
                    throw new BadStateException($"{error}");
            }
        }

        public Result<T, E> AndThen(Func<T, Result<T, E>> next)
        {
            switch (Kind)
            {
                case Result.Ok:
                    return next(value);
                default:
                    return this;
            }
        }

        public static implicit operator T(Result<T, E> result)
        {
            if (result.Kind is Result.Error)
            {
                throw new BadStateException(
                    "Cannot get the results value because it is in an error state."
                );
            }

            return result.value;
        }

        public static implicit operator Result(Result<T, E> result) => result.Kind;

        public static implicit operator bool(Result<T, E> result) => result.Kind == Result.Ok;
    }
}
