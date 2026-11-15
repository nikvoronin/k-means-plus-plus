using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace KMeans.DistanceEstimators;

/// <summary>
/// Chebyshev distance: max|aᵢ - bᵢ|.
/// </summary>
public readonly struct ChebyshevDistance<T> : IDistanceEstimator<T>
    where T : unmanaged, INumber<T>
{
    public bool IsMetric => true;

    public T Estimate(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
    {
        if (a.Length != b.Length)
        {
            ThrowHelper.DimensionMismatch(a.Length, b.Length);
        }

        var distance = T.Zero;
        var i = 0;

        if (Vector.IsHardwareAccelerated
            && Vector<T>.IsSupported
            && a.Length >= Vector<T>.Count)
        {
            var va = MemoryMarshal.Cast<T, Vector<T>>(a);
            var vb = MemoryMarshal.Cast<T, Vector<T>>(b);
            var vmax = Vector<T>.Zero;

            for (var v = 0; v < va.Length; v++)
            {
                vmax = Vector.Max(vmax, Vector.Abs(va[v] - vb[v]));
            }

            for (var lane = 0; lane < Vector<T>.Count; lane++)
            {
                distance = T.Max(distance, vmax[lane]);
            }

            i = va.Length * Vector<T>.Count;
        }

        for (; i < a.Length; i++)
        {
            distance = T.Max(distance, T.Abs(a[i] - b[i]));
        }

        return distance;
    }

    public T Compare(ReadOnlySpan<T> a, ReadOnlySpan<T> b) => Estimate(a, b);
}
