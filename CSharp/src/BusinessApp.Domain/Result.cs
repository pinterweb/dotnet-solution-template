namespace BusinessApp.Domain
{
    /// <summary>
    /// Indicator for success/failure
    /// </summary>
    public enum Result
    {
        Ok,
        Failure
    }

    /// <summary>
    /// Wraps a value with the outcome, normally from enforcing invariants
    /// and provides access to it or throws an exception
    /// </summary>
    public struct Result<T>
    {
        private readonly T value;
        public readonly Result Kind;

        public static Result<T> Ok(T value) => new Result<T>(value, Result.Ok);
        public static Result<T> Failure(T value) => new Result<T>(value, Result.Failure);

        private Result(T value, Result result)
        {
            this.value = value;
            this.Kind = result;
        }

        public T Expect(string message)
        {
            try
            {
                return (T)this;
            }
            catch (BadStateException e)
            {
                throw new BadStateException(message, e);
            }
        }

        public static implicit operator T(Result<T> result)
        {
            if (result.Kind is Result.Failure)
            {
                throw new BadStateException(
                    "Cannot get the value because it is invalid in the current context"
                );
            }

            return result.value;
        }

        public static implicit operator Result(Result<T> result) => result.Kind;
    }
}
