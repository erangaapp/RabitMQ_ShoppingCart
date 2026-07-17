namespace SharedKernel;

/// <summary>Base class for all entities. Identity-based equality.</summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public override bool Equals(object? obj) =>
        obj is Entity other && GetType() == other.GetType() && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}

/// <summary>Marker for DDD aggregate roots — the only entities repositories load/save directly.</summary>
public interface IAggregateRoot;
