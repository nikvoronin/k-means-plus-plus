using System;
using System.Numerics;

namespace KMeans;

/// <summary>
/// Distance between two vectors of the same dimension.
/// Implementations are expected to be stateless <see langword="struct"/>s,
/// so the JIT can specialize and inline them in the hot loop.
/// </summary>
public interface IDistanceEstimator<T>
    where T : unmanaged, INumber<T>
{
    /// <summary>
    /// Distance in the units of the metric.
    /// Used for the convergence epsilon and the K-Means++ seeding.
    /// </summary>
    T Estimate(ReadOnlySpan<T> a, ReadOnlySpan<T> b);

    /// <summary>
    /// Value that orders the same way as <see cref="Estimate"/> but is cheaper to compute
    /// (for example, the squared Euclidean distance). Used to find the nearest centroid.
    /// </summary>
    T Compare(ReadOnlySpan<T> a, ReadOnlySpan<T> b);
}
