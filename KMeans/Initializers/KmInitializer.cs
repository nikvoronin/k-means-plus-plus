using System;
using System.Collections.Generic;
using System.Numerics;

namespace KMeans.Initializers;

/// <summary>
/// Simple initializer: centroids are distinct points chosen uniformly at random.
/// </summary>
public sealed class KmInitializer<T> : IKmInitializer<T>
    where T : unmanaged, INumber<T>
{
    public void Initialize<TDistance>(
        ReadOnlyMemory<T> points,
        ReadOnlyMemory<int> weights,
        int dimension,
        Span<T> centroids,
        TDistance distance,
        Random random)
        where TDistance : struct, IDistanceEstimator<T>
    {
        var n = points.Length / dimension;
        var clusterCount = centroids.Length / dimension;
        var chosen = new HashSet<int>(clusterCount);

        for (var c = 0; c < clusterCount; c++)
        {
            int index;
            do index = random.Next(n);
            while (!chosen.Add(index));

            points.Span.Slice(index * dimension, dimension)
                .CopyTo(centroids.Slice(c * dimension, dimension));
        }
    }
}
