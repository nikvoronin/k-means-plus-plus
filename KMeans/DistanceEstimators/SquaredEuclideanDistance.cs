using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KMeans.DistanceEstimators;

/// <summary>
/// Squared Euclidean distance: Σ(aᵢ - bᵢ)².
/// </summary>
public readonly struct SquaredEuclideanDistance<T> : IDistanceEstimator<T>
    where T : unmanaged, INumber<T>
{
    public T Estimate(ReadOnlySpan<T> a, ReadOnlySpan<T> b) => Sum(a, b);

    public T Compare(ReadOnlySpan<T> a, ReadOnlySpan<T> b) => Sum(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Sum(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
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
                var d = va[v] - vb[v];
                vacc += d * d;
            }

            acc = Vector.Sum(vacc);
            i = va.Length * Vector<T>.Count;
        }

        for (; i < a.Length; i++)
        {
            var d = a[i] - b[i];
            acc += d * d;
        }

        return acc;
    }
}
