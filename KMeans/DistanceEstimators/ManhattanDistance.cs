using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace KMeans.DistanceEstimators;

/// <summary>
/// Manhattan distance: Σ|aᵢ - bᵢ|.
/// </summary>
public readonly struct ManhattanDistance<T> : IDistanceEstimator<T>
    where T : unmanaged, INumber<T>
{
    public bool IsMetric => true;

    public T Estimate(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
    {
        if (a.Length != b.Length)
        {
            ThrowHelper.DimensionMismatch(a.Length, b.Length);
        }

        var acc = T.Zero;
        var i = 0;

        if (Vector.IsHardwareAccelerated
            && Vector<T>.IsSupported
            && a.Length >= Vector<T>.Count)
        {
            var va = MemoryMarshal.Cast<T, Vector<T>>(a);
            var vb = MemoryMarshal.Cast<T, Vector<T>>(b);
            var vacc = Vector<T>.Zero;

            for (var v = 0; v < va.Length; v++)
            {
                vacc += Vector.Abs(va[v] - vb[v]);
            }

            acc = Vector.Sum(vacc);
            i = va.Length * Vector<T>.Count;
        }

        for (; i < a.Length; i++)
        {
            acc += T.Abs(a[i] - b[i]);
        }

        return acc;
    }

    public T Compare(ReadOnlySpan<T> a, ReadOnlySpan<T> b) => Estimate(a, b);
}
