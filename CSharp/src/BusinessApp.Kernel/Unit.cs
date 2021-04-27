using System;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Represents nothing, useful when a return type is necessary with <see cref="Result{T, E}"/>
    /// </summary>
    public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
    {
        private static readonly Unit unit;

        public static ref readonly Unit Value => ref unit;

        public int CompareTo(Unit other) => 0;
        public int CompareTo(object? obj) => 0;
        public bool Equals(Unit other) => true;
        public override bool Equals(object? obj) => obj is Unit;
        public override int GetHashCode() => 0;
        public override string ToString() => "()";
        public static bool operator ==(Unit left, Unit right) => left.Equals(right);
        public static bool operator !=(Unit left, Unit right) => !(left == right);
        public static bool operator <(Unit left, Unit right) => left.CompareTo(right) < 0;
        public static bool operator <=(Unit left, Unit right) => left.CompareTo(right) <= 0;
        public static bool operator >(Unit left, Unit right) => left.CompareTo(right) > 0;
        public static bool operator >=(Unit left, Unit right) => left.CompareTo(right) >= 0;
    }
}
