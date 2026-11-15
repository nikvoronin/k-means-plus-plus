using System;
using System.Numerics;

namespace KMeans.DistanceEstimators;

/// <summary>
/// Euclidean distance: √Σ(aᵢ - bᵢ)². Nearest centroid search skips the square root.
/// </summary>
public readonly struct EuclideanDistance<T> : IDistanceEstimator<T>
    where T : unmanaged, INumber<T>, IRootFunctions<T>
{
    public T Estimate( ReadOnlySpan<T> a, ReadOnlySpan<T> b ) =>
        T.Sqrt( SquaredEuclideanDistance<T>.Sum( a, b ) );

    public T Compare( ReadOnlySpan<T> a, ReadOnlySpan<T> b ) =>
        SquaredEuclideanDistance<T>.Sum( a, b );
}
