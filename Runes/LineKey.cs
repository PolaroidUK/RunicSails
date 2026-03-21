using System;

public readonly struct LineKey : IEquatable<LineKey>
{
    public int A { get; }
    public int B { get; }

    public LineKey(int a, int b)
    {
        if (a < b)
        {
            A = a;
            B = b;
        }
        else
        {
            A = b;
            B = a;
        }
    }

    public bool Equals(LineKey other) => A == other.A && B == other.B;

    public override bool Equals(object? obj) => obj is LineKey other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(A, B);

    public override string ToString() => $"{A}-{B}";
}