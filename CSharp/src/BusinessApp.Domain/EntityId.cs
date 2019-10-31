namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// Represents the unique id of any Entity
    /// </summary>
    public abstract class EntityId<TId> : IEntityId, IComparable<EntityId<TId>>
        where TId : IComparable
    {
        public EntityId() { }

        public EntityId(TId value)
        {
            Id = value;
        }

        public TId Id { get; set; }

        public virtual bool IsNew() => Id.CompareTo(default(TId)) == 0;

        public int CompareTo(EntityId<TId> other)
        {
            if (other == null) return 1;

            return Id.CompareTo(other.Id);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is EntityId<TId> id)
            {
                CompareTo(id);
            }

            throw new ArgumentException($"Object is not the same type");
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var aggregate = obj as EntityId<TId>;

            return Id.CompareTo(default(TId)) != 0 ? CompareTo(aggregate) == 0 : base.Equals(obj);
        }

        public override int GetHashCode() => Id.GetHashCode();
        public override string ToString() => Id.ToString();

        public static bool operator ==(EntityId<TId> a, EntityId<TId> b)
        {
            if (ReferenceEquals(a, b)) return true;

            return a is null ? false : a.Equals(b);
        }

        public static bool operator !=(EntityId<TId> a, EntityId<TId> b)
        {
            return !(a == b);
        }

        public static implicit operator TId(EntityId<TId> id) => id.Id;
    }
}
