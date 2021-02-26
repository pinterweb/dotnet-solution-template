namespace BusinessApp.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Wraps an outcomes that disregards the value
    /// while keeping the error
    /// </summary>
    public struct Result : IEquatable<Result>, IComparable<Result>, IFormattable
    {
        private readonly IFormattable error;

        public static readonly Result Ok = new Result(null);

        public static Result Error(IFormattable error)
        {
            error.NotNull().Expect(nameof(error));

            return new Result(error);
        }

        public static Result<T, IFormattable> From<T>(Func<T> func)
        {
            try
            {
                var val = func();
                return Result<T>.Ok(val);
            }
            catch (Exception e) when (e is IFormattable ef)
            {
                return Result<T>.Error(ef);
            }
        }

        private Result(IFormattable error)
        {
            this.error = error;
            Kind = error == null ? ValueKind.Ok : ValueKind.Error;
        }

        public ValueKind Kind { get; }

        public Result<IFormattable, IFormattable> Into()
        {
            return Kind switch
            {
                ValueKind.Ok => Result<IFormattable, IFormattable>.Ok(null),
                ValueKind.Error => Result<IFormattable, IFormattable>.Error(error),
                _ => throw new NotImplementedException()
            };
        }

        public Result<T, IFormattable> Into<T>()
        {
            return Kind switch
            {
                ValueKind.Ok => Result<T, IFormattable>.Ok(default),
                ValueKind.Error => Result<T, IFormattable>.Error(error),
                _ => throw new NotImplementedException()
            };
        }

        public bool Equals(Result other)
        {
            return Kind switch
            {
                ValueKind.Ok => other.Kind == ValueKind.Ok,
                ValueKind.Error =>
                    (error == null && other.error == null) ||
                    error.ToString().Equals(other.error.ToString(null, null)),
                _ => throw new NotImplementedException(),
            };
        }

        public int CompareTo(Result other)
        {
            return Kind switch
            {
                ValueKind.Ok => other.Kind == ValueKind.Ok ? 0 : -1,
                ValueKind.Error => other.Kind == ValueKind.Ok
                    ? 1
                    : error.ToString("G", null).CompareTo(other.error.ToString("G", null)),
                _ => throw new NotImplementedException(),
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is Result other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                    // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;
                int hash = (HashingBase * HashingMultiplier) ^ Kind.GetHashCode();

                return (hash * HashingMultiplier) ^ error?.GetHashCode() ?? 0;
            }
        }

        public override string ToString()
        {
            return ToString("G", null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Kind switch
            {
                ValueKind.Error => error.ToString(format, formatProvider),
                ValueKind.Ok => "",
                _ => throw new NotImplementedException()
            };
        }

        public static implicit operator Result(bool result) =>
            result ? Result.Ok : Result.Error($"False");
    }

    /// <summary>
    /// Wraps a value with the outcome, normally from enforcing invariants
    /// and provides access to it or can throw an exception
    /// </summary>
    public readonly struct Result<T> : IEquatable<Result<T>>, IComparable<Result<T>>
    {
        private readonly T value;
        private readonly IFormattable error;

        public static Result<T> Ok(T value) => new Result<T>(value, default, ValueKind.Ok);
        public static Result<T> Error(IFormattable error) => new Result<T>(default, error, ValueKind.Error);

        private Result(T value = default, IFormattable error = default, ValueKind result = default)
        {
            this.value = value;
            this.error = error;
            this.Kind = result;
        }

        public ValueKind Kind { get; }

        public Result<T, IFormattable> Into()
        {
            return Kind switch
            {
                ValueKind.Ok => Result<T, IFormattable>.Ok(value),
                ValueKind.Error => Result<T, IFormattable>.Error(error),
                _ => throw new NotImplementedException()
            };
        }

        public bool Equals(Result<T> other)
        {
            if (other.Kind != Kind) return false;

            return Kind switch
            {
                ValueKind.Ok =>
                    (value == null && other.value == null) ||
                    value.Equals(other.value),
                ValueKind.Error =>
                    (error == null && other.error == null) ||
                    error.ToString().Equals(other.error.ToString(null, null)),
                _ => throw new NotImplementedException(),
            };
        }

        public int CompareTo(Result<T> other)
        {
            if (Kind != other.Kind) return Kind.CompareTo(other.Kind);

            return Kind switch
            {
                ValueKind.Ok => Comparer<T>.Default.Compare(value, other.value),
                ValueKind.Error => other.Kind == ValueKind.Ok
                    ? 1
                    : error.ToString("G", null).CompareTo(other.error.ToString("G", null)),
                _ => throw new NotImplementedException(),
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is Result<T> other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                    // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;
                int hash = (HashingBase * HashingMultiplier) ^ Kind.GetHashCode();

                return (hash * HashingMultiplier) ^  Kind switch
                {
                    ValueKind.Error => error.GetHashCode(),
                    ValueKind.Ok => value.GetHashCode(),
                    _ => throw new NotImplementedException()
                };
            }
        }

        public static implicit operator Result(Result<T> result) =>
            result.Kind == ValueKind.Ok ? Result.Ok : Result.Error(result.Into().UnwrapError());

        public static implicit operator Result<T, IFormattable>(Result<T> result) =>
            result.Kind == ValueKind.Ok ?
            Result<T, IFormattable>.Ok(result.value) :
            Result<T, IFormattable>.Error(result.error);
    }

    /// <summary>
    /// Wraps a value with the outcome, normally from enforcing invariants
    /// and provides access to it or can throw an exception
    /// </summary>
    public struct Result<T, E> : IEquatable<Result<T, E>>, IComparable<Result<T, E>>
        where E : IFormattable
    {
        private readonly T value;
        private readonly E error;

        public static Result<T, E> Ok(T value) => new Result<T, E>(value, default, ValueKind.Ok);
        public static Result<T, E> Error(E error) => new Result<T, E>(default, error, ValueKind.Error);

        private Result(T value = default, E error = default, ValueKind result = default)
        {
            this.value = value;
            this.error = error;
            this.Kind = result;
        }

        public ValueKind Kind { get; }

        public T Expect(string message)
        {
            return Kind switch
            {
                ValueKind.Ok => value,
                ValueKind.Error => throw new BadStateException($"{message}: {error}"),
                    _ => throw new NotImplementedException(),
            };
        }

        public T Expect<TException>(string message) where TException : Exception
        {
            try
            {
                return Kind switch
                {
                    ValueKind.Ok => value,
                    ValueKind.Error =>
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
                ValueKind.Ok => throw new BadStateException($"{message}: {value}"),
                ValueKind.Error => error,
                _ => throw new NotImplementedException(),
            };
        }

        public E UnwrapError()
        {
            return Kind switch
            {
                ValueKind.Ok => throw new BadStateException($"{value}"),
                ValueKind.Error => error,
                _ => throw new NotImplementedException(),
            };
        }

        public T Unwrap()
        {
            return Kind switch
            {
                ValueKind.Ok => value,
                ValueKind.Error => throw new BadStateException($"{error}"),
                _ => throw new NotImplementedException(),
            };
        }

        public Result<T, E> Or(Result<T, E> other)
        {
            return Kind switch
            {
                ValueKind.Ok => this,
                ValueKind.Error => other,
                _ => throw new NotImplementedException(),
            };
        }

        public Result<T, E> AndThen(Func<T, Result<T, E>> next)
        {
            return Kind switch
            {
                ValueKind.Ok => next(value),
                ValueKind.Error => this,
                _ => throw new NotImplementedException(),
            };
        }

        public Result<T, E> ThenRun<R>(Func<T, R> next)
        {
            if (Kind == ValueKind.Ok)
            {
                var _ = next(value);
            }

            return this;
        }

        public Result<T, E> ThenRun(Action<T> next)
        {
            if (Kind == ValueKind.Ok)
            {
                next(value);
            }

            return this;
        }

        public Result<T, E> OrElse(Func<E, Result<T, E>> onError)
        {
            return Kind switch
            {
                ValueKind.Error => onError(error),
                ValueKind.Ok => this,
                _ => throw new NotImplementedException(),
            };
        }

        public Result<R, E> Map<R>(Func<T, R> onOk)
        {
            return Kind switch
            {
                ValueKind.Error => Result<R, E>.Error(error),
                ValueKind.Ok => Result<R, E>.Ok(onOk(value)),
                _ => throw new NotImplementedException(),
            };
        }

        public R MapOrElse<R>(Func<E, R> onError, Func<T, R> onOk)
        {
            return Kind switch
            {
                ValueKind.Error => onError(error),
                ValueKind.Ok => onOk(value),
                _ => throw new NotImplementedException(),
            };
        }

        public Result Into()
        {
            return Kind switch
            {
                ValueKind.Ok => Result.Ok,
                ValueKind.Error => Result.Error(error),
                _ => throw new NotImplementedException()
            };
        }

        public bool Equals(Result<T, E> other)
        {
            if (other.Kind != Kind) return false;

            return Kind switch
            {
                ValueKind.Ok =>
                    (value == null && other.value == null) ||
                    value.Equals(other.value),
                ValueKind.Error =>
                    (error == null && other.error == null) ||
                    error.ToString().Equals(other.error.ToString(null, null)),
                _ => throw new NotImplementedException(),
            };
        }

        public int CompareTo(Result<T, E> other)
        {
            if (Kind != other.Kind) return Kind.CompareTo(other.Kind);

            return Kind switch
            {
                ValueKind.Ok => Comparer<T>.Default.Compare(value, other.value),
                ValueKind.Error => Comparer<E>.Default.Compare(error, other.error),
                _ => throw new NotImplementedException(),
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is Result<T, E> other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                    // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;
                int hash = (HashingBase * HashingMultiplier) ^ Kind.GetHashCode();

                return (hash * HashingMultiplier) ^  Kind switch
                {
                    ValueKind.Error => error.GetHashCode(),
                    ValueKind.Ok => value.GetHashCode(),
                    _ => throw new NotImplementedException()
                };
            }
        }

        public static explicit operator T(Result<T, E> result)
        {
            return result.Expect("Cannot get the value because it is an error");
        }

        public static implicit operator Result(Result<T, E> result)
        {
            return result.MapOrElse(
                err => Result.Error(err),
                _ => Result.Ok
            );
        }

        public static implicit operator Result<T>(Result<T, E> result)
        {
            return result.MapOrElse(
                err => Result<T>.Error(err),
                val => Result<T>.Ok(val)
            );
        }

        public static explicit operator E(Result<T, E> result)
        {
            return result.ExpectError("Cannot get the error because it is a valid value");
        }

        public static implicit operator ValueKind(Result<T, E> result) => result.Kind;

        public static implicit operator bool(Result<T, E> result) => result.Kind == ValueKind.Ok;
    }
}
