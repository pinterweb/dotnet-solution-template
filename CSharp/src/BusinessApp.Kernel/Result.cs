using System;
using System.Collections.Generic;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Factory methods using a general exception as the error
    /// </summary>
    public static class Result
    {
        private static readonly Result<Unit, Exception> emptyOk
            = Result<Unit, Exception>.Ok(Unit.Value);

        public static ref readonly Result<Unit, Exception> Ok() => ref emptyOk;

        public static Result<T, Exception> Ok<T>(T okVal) => Result<T, Exception>.Ok(okVal);

        public static Result<T, TErr> Ok<T, TErr>(T okVal) => Result<T, TErr>.Ok(okVal);

        public static Result<Unit, Exception> Error(Exception error) => Result<Unit, Exception>.Error(error);

        public static Result<T, Exception> Error<T>(Exception error) => Result<T, Exception>.Error(error);

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
    public readonly struct Result<T, TErr> : IEquatable<Result<T, TErr>>, IComparable<Result<T, TErr>>
    {
        private readonly T? value;
        private readonly TErr? error;

        private Result(T? value, TErr? error, ValueKind result)
        {
            this.value = value;
            this.error = error;
            Kind = result;
        }

#pragma warning disable CA1000
        public static Result<T, TErr> Ok(T val) => new(val, default, ValueKind.Ok);

        public static Result<T, TErr> Error(TErr error) => error == null
            ? throw new BusinessAppException("A error result must have an error value")
            : new(default, error, ValueKind.Error);
#pragma warning restore CA1000

        public ValueKind Kind { get; }

        public T Expect(string message) => Kind switch
        {
            ValueKind.Ok => value!,
            ValueKind.Error => throw new BusinessAppException($"{message}: {error}"),
            _ => throw new NotImplementedException(),
        };

        public TErr ExpectError(string message) => Kind switch
        {
            ValueKind.Ok => throw new BusinessAppException($"{message}: {value}"),
            ValueKind.Error => error!,
            _ => throw new NotImplementedException(),
        };

        public TErr UnwrapError() => Kind switch
        {
            ValueKind.Ok => throw new BusinessAppException($"{value}"),
            ValueKind.Error => error!,
            _ => throw new NotImplementedException(),
        };

        public T Unwrap() => Kind switch
        {
            ValueKind.Ok => value!,
            ValueKind.Error => throw new BusinessAppException($"{error}"),
            _ => throw new NotImplementedException(),
        };

        public Result<T, TErr> Or(Result<T, TErr> other) => Kind switch
        {
            ValueKind.Ok => this,
            ValueKind.Error => other,
            _ => throw new NotImplementedException(),
        };

        public Result<TOut, TErr> AndThen<TOut>(Func<T, Result<TOut, TErr>> next) => Kind switch
        {
            ValueKind.Ok => next(value!),
            ValueKind.Error => Result<TOut, TErr>.Error(error!),
            _ => throw new NotImplementedException(),
        };

        public Result<T, TErr> OrElse(Func<TErr, Result<T, TErr>> onError) => Kind switch
        {
            ValueKind.Error => onError(error!),
            ValueKind.Ok => this,
            _ => throw new NotImplementedException(),
        };

        public Result<TOut, TErr> Map<TOut>(Func<T, TOut> onOk) => Kind switch
        {
            ValueKind.Error => Result<TOut, TErr>.Error(error!),
            ValueKind.Ok => Result<TOut, TErr>.Ok(onOk(value!)),
            _ => throw new NotImplementedException(),
        };

        public Result<T, TErrOut> MapError<TErrOut>(Func<TErr, TErrOut> onErr) => Kind switch
        {
            ValueKind.Error => Result<T, TErrOut>.Error(onErr(error!)),
            ValueKind.Ok => Result<T, TErrOut>.Ok(value!),
            _ => throw new NotImplementedException(),
        };

        public TOut MapOrElse<TOut>(Func<TErr, TOut> onError, Func<T, TOut> onOk) => Kind switch
        {
            ValueKind.Error => onError(error!),
            ValueKind.Ok => onOk(value!),
            _ => throw new NotImplementedException(),
        };

        public bool Equals(Result<T, TErr> other)
            => other.Kind == Kind
            && Kind switch
            {
                ValueKind.Ok =>
                    (value == null && other.value == null) ||
                    value!.Equals(other.value),
                ValueKind.Error =>
                    (error == null && other.error == null) ||
                    error!.Equals(other.error),
                _ => throw new NotImplementedException(),
            };

        public int CompareTo(Result<T, TErr> other) => Kind != other.Kind
            ? Kind.CompareTo(other.Kind)
            : Kind switch
            {
                ValueKind.Ok => Comparer<T>.Default.Compare(value, other.value),
                ValueKind.Error => Comparer<TErr>.Default.Compare(error, other.error),
                _ => throw new NotImplementedException(),
            };

        public override bool Equals(object? obj) => obj is Result<T, TErr> other
            ? Equals(other)
            : base.Equals(obj);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                const int multiplier = 23;

                hash = (hash * multiplier) + Kind.GetHashCode();

                return (hash * multiplier) + Kind switch
                {
                    ValueKind.Error => error!.GetHashCode(),
                    ValueKind.Ok => value!.GetHashCode(),
                    _ => throw new NotImplementedException()
                };
            }
        }

        public static explicit operator T(Result<T, TErr> result)
            => result.Expect("Cannot get the value because it is an error");

        public static explicit operator TErr(Result<T, TErr> result)
            => result.ExpectError("Cannot get the error because it is a valid value");

        public static implicit operator ValueKind(Result<T, TErr> result) => result.Kind;

        public static implicit operator bool(Result<T, TErr> result) => result.Kind == ValueKind.Ok;

        public static bool operator ==(Result<T, TErr> left, Result<T, TErr> right)
            => left.Equals(right);

        public static bool operator !=(Result<T, TErr> left, Result<T, TErr> right)
            => !(left == right);

        public static bool operator <(Result<T, TErr> left, Result<T, TErr> right)
            => left.CompareTo(right) < 0;

        public static bool operator <=(Result<T, TErr> left, Result<T, TErr> right)
            => left.CompareTo(right) <= 0;

        public static bool operator >(Result<T, TErr> left, Result<T, TErr> right)
            => left.CompareTo(right) > 0;

        public static bool operator >=(Result<T, TErr> left, Result<T, TErr> right)
            => left.CompareTo(right) >= 0;
    }
}
