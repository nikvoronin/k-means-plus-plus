using System;
using System.Numerics;

namespace KMeans;

public interface IKmInitializer<T>
    where T : unmanaged, INumber<T>
{
    /// <summary>
    /// Choose the initial centroids.
    /// </summary>
    /// <param name="points">
    /// Points, row-major: n × <paramref name="dimension"/>.
    /// </param>
    /// <param name="weights">
    /// Weight of every point, or empty when all weights are 1.
    /// </param>
    /// <param name="dimension">
    /// Dimension of the vectors.
    /// </param>
    /// <param name="centroids">
    /// Output, row-major: k × <paramref name="dimension"/>.
    /// </param>
    /// <param name="distance">
    /// Distance estimator.
    /// </param>
    /// <param name="random">
    /// Randomizer.
    /// </param>
    void Initialize<TDistance>(
        ReadOnlyMemory<T> points,
        ReadOnlyMemory<int> weights,
        int dimension,
        Span<T> centroids,
        TDistance distance,
        Random random)
        where TDistance : struct, IDistanceEstimator<T>;
}
