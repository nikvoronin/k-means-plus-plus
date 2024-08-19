using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace KMeans.Models;

public class VectorN<T> : IEquatable<VectorN<T>>
    where T : INumber<T>
{
    public readonly static VectorN<T> Empty = new();
    public static VectorN<T> Zero( int dimension ) =>
        new( new T[dimension] );

    public int Dimension;

    public T this[Index index] => _values[index];
    public IReadOnlyList<T> Components => _values;

    private VectorN()
    {
        _values = [];
        Dimension = 0;
    }

    public VectorN( params T[] values )
    {
        var vals = values ?? [];
        _values = new T[vals.Length];
        Array.Copy( vals, _values, vals.Length );

        Dimension = _values.Length;
    }

    public VectorN<T> Clone() => new( _values );

    public bool Equals( [AllowNull] VectorN<T> other )
    {
        if (other is null) return false;

        if (ReferenceEquals( this, other )) return true;

        if (other._values.Length != _values.Length) return false;

        for (int i = 0; i < Dimension; i++)
            if (other._values[i] != _values[i]) return false;

        return true;
    }

    public override bool Equals( [AllowNull] object obj )
    {
        if (obj is not VectorN<T>) return false;

        return Equals( (VectorN<T>)obj );
    }

    public override int GetHashCode() => _values.GetHashCode();

    public static VectorN<T> operator +( VectorN<T> left, VectorN<T> right )
    {
        if (left is null && right is null) return Empty;

        if (left is null || left == Empty) return right;
        if (right is null || right == Empty) return left;

        if (left.Dimension != right.Dimension) {
            throw new ArgumentOutOfRangeException(
                $"Vectors dimension mismatch: {left.Dimension} <> {right.Dimension}" );
        }

        T[] result = new T[left._values.Length];
        for (var i = 0; i < result.Length; i++)
            result[i] = left._values[i] + right._values[i];

        return new( result );
    }

    public static VectorN<T> operator /( VectorN<T> value1, T value2 )
    {
        T invDiv = T.One / value2;

        T[] result = new T[value1.Dimension];
        for (var i = 0; i < result.Length; i++)
            result[i] = value1._values[i] * invDiv;

        return new( result );
    }

    public static bool operator ==( VectorN<T> left, VectorN<T> right ) =>
        left.Equals( right );

    public static bool operator !=( VectorN<T> left, VectorN<T> right ) =>
        !left.Equals( right );

    private readonly T[] _values;
}
