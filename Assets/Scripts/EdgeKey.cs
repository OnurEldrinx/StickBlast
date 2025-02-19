using System;

[Serializable]
public struct EdgeKey : IEquatable<EdgeKey>
{
    public int Id1 { get; }
    public int Id2 { get; }

    public EdgeKey(int id1, int id2)
    {
        if (id1 < id2)
        {
            Id1 = id1;
            Id2 = id2;
        }
        else
        {
            Id1 = id2;
            Id2 = id1;
        }
    }

    public bool Equals(EdgeKey other)
    {
        return Id1 == other.Id1 && Id2 == other.Id2;
    }

    public override bool Equals(object obj)
    {
        return obj is EdgeKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Id1 * 397) ^ Id2;
        }
    }

    public override string ToString() => $"{Id1},{Id2}";
}