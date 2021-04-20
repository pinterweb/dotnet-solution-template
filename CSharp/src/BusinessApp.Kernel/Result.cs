using System;
using System.Collections.Generic;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Factory methods using a general exception as the error
    /// </summary>
    public static class Result
    {
        public static Result<Unit, Exception> OK => Result<Unit, Exception>.Ok(Unit.New);

        public static Result<T, Exception> Ok<T>(T okVal)
        {
            return Result<T, Exception>.Ok(okVal);
        }

        public static Result<Unit, Exception> Error(Exception error)
        {
            return Result<Unit, Exception>.Error(error);
        }

        public static Result<T, Exception> Error<T>(Exception error)
        {
            return Result<T, Exception>.Error(error);
        }

        public static Result<T, Exception> From<T>(Func<T> func)
        {
            try
            {
                var val = func();
                return Result<T, Exception>.Ok(val);
            }
            catch (Exception e)
            {
                return Result<T, Exception>.Error(e);
            }
        }
    }

    /// <summary>
    /// Wraps a value with the outcome, normally from enforcing invariants
    /// and provides access to it or can throw an exception
    /// </summary>
    public readonly struct Result<T, E> : IEquatable<Result<T, E>>, IComparable<Result<T, E>>
    {
        private readonly T? value;
        private readonly E? error;

        private Result(T? value, E? error, ValueKind result)
        {
            this.value = value;
            this.error = error;
            this.Kind = result;
        }

        public static Result<T, E> Ok(T val)
        {
            return new Result<T, E>(val, default, ValueKind.Ok);
        }

        public static Result<T, E> Error(E error)
        {
            return error == null
                ? throw new BusinessAppException("A error result must have an error value")
                : new Result<T, E>(default, error, ValueKind.Error);
        }

        public ValueKind Kind { get; }

        public T Expect(string message)
        {
            return Kind switch
            {
                ValueKind.Ok => value!,
                ValueKind.Error => throw new BusinessAppException($"{message}: {error}"),
                    _ => throw new NotImplementedException(),
            };
        }

        public E ExpectError(string message)
        {
            return Kind switch
            {
                ValueKind.Ok => throw new BusinessAppException($"{message}: {value}"),
                ValueKind.Error => error!,
                _ => throw new NotImplementedException(),
            };
        }

        public E UnwrapError()
        {
            return Kind switch
            {
                ValueKind.Ok => throw new BusinessAppException($"{value}"),
                ValueKind.Error => error!,
                _ => throw new NotImplementedException(),
            };
        }

        public T Unwrap()
        {
            return Kind switch
            {
                ValueKind.Ok => value!,
                ValueKind.Error => throw new BusinessAppException($"{error}"),
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

        public Result<U, E> AndThen<U>(Func<T, Result<U, E>> next)
        {
            return Kind switch
            {
                ValueKind.Ok => next(value!),
                ValueKind.Error => Result<U, E>.Error(error!),
                _ => throw new NotImplementedException(),
            };
        }

        public Result<T, E> OrElse(Func<E, Result<T, E>> onError)
        {
            return Kind switch
            {
                ValueKind.Error => onError(error!),
                ValueKind.Ok => this,
                _ => throw new NotImplementedException(),
            };
        }

        public Result<R, E> Map<R>(Func<T, R> onOk)
        {
            return Kind switch
            {
                ValueKind.Error => Result<R, E>.Error(error!),
                ValueKind.Ok => Result<R, E>.Ok(onOk(value!)),
                _ => throw new NotImplementedException(),
            };
        }

        public Result<T, R> MapError<R>(Func<E, R> onErr)
        {
            return Kind switch
            {
                ValueKind.Error => Result<T, R>.Error(onErr(error!)),
                ValueKind.Ok => Result<T, R>.Ok(value!),
                _ => throw new NotImplementedException(),
            };
        }

        public R MapOrElse<R>(Func<E, R> onError, Func<T, R> onOk)
        {
            return Kind switch
            {
                ValueKind.Error => onError(error!),
                ValueKind.Ok => onOk(value!),
                _ => throw new NotImplementedException(),
            };
        }

        public bool Equals(Result<T, E> other)
        {
            if (other.Kind != Kind) return false;

            return Kind switch
            {
                ValueKind.Ok =>
                    (value == null && other.value == null) ||
                    value!.Equals(other.value),
                ValueKind.Error =>
                    (error == null && other.error == null) ||
                    error!.Equals(other.error),
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

        public override bool Equals(object? obj)
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
                    ValueKind.Error => error!.GetHashCode(),
                    ValueKind.Ok => value!.GetHashCode(),
                    _ => throw new NotImplementedException()
                };
            }
        }

        public static explicit operator T(Result<T, E> result)
        {
            return result.Expect("Cannot get the value because it is an error");
        }

        public static explicit operator E(Result<T, E> result)
        {
            return result.ExpectError("Cannot get the error because it is a valid value");
        }

        public static implicit operator ValueKind(Result<T, E> result) => result.Kind;

        public static implicit operator bool(Result<T, E> result) => result.Kind == ValueKind.Ok;
    }
}
