using System;
using System.Numerics;
using KMeans.Models;

namespace KMeans.Initializers;

/// <summary>
/// Simple cluster initializer.
/// </summary>
public class KmInitializer<T> : IKmInitializer<T>
    where T : INumber<T>
{
    /// <summary>
    /// Initialize first iteration by choosing random Centroid volume
    /// within the given array of volume.
    /// </summary>
    /// <param name="volume">
    /// Vector volume.
    /// </param>
    /// <param name="random">
    /// Randomizer.
    /// </param>
    /// <returns>
    /// Initial cluster set.
    /// </returns>
    public KmCluster<T>[] Initialize(
        VectorN<T>[] volume, int numClusters,
        IDistanceEstimator<T> distanceEstimator,
        Random? random = null)
    {
        random ??= Random.Shared;

        KmCluster<T>[] clusters = new KmCluster<T>[numClusters];
        for (var i = 0; i < numClusters; i++)
            clusters[i] = new KmCluster<T>(volume[random.Next(volume.Length)]);

        return clusters;
    }
}
