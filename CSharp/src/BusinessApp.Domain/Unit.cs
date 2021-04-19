using System;

namespace BusinessApp.Domain
{
    /// <summary>
    /// Represents nothing, useful when a return type is necessary with <see cref="Result{T, E}"/>
    /// </summary>
    public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
    {
        public static Unit New => default;

        public int CompareTo(Unit other) => 0;
        public int CompareTo(object? obj) => 0;
        public bool Equals(Unit other) => true;
        public override bool Equals(object? obj) => obj is Unit;
        public override int GetHashCode() => 0;
        public override string ToString() => "()";
        public static bool operator ==(Unit first, Unit second) => true;
        public static bool operator !=(Unit first, Unit second) => false;
    }
}
