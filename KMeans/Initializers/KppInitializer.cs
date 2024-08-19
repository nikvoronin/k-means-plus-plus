using System;
using System.Numerics;
using KMeans.Models;

namespace KMeans.Initializers;

/// <summary>
/// KMeans++ cluster initializer.
/// </summary>
public class KppInitializer<T> : IKmInitializer<T>
    where T : INumber<T>
{
    /// <summary>
    /// Initialize first iteration by choosing first Centroid point at random
    /// and next ones with the K-Means++ algorithm.
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
        for (var i = 0; i < clusters.Length; i++)
            clusters[i] = new();

        clusters[0] = new KmCluster<T>(volume[random.Next(volume.Length)]);

        for (var i = 1; i < numClusters; i++)
        {
            var accumulatedDistances = T.Zero;

            T[] accDistances = new T[volume.Length];

            for (var pointIdx = 0; pointIdx < volume.Length; pointIdx++)
            {
                var minDistance =
                    distanceEstimator.Estimate(
                        clusters[0].Centroid,
                        volume[pointIdx]);

                for (var clusterIdx = 1; clusterIdx < i; clusterIdx++)
                {
                    var currentDistance =
                        distanceEstimator.Estimate(
                            clusters[clusterIdx].Centroid,
                            volume[pointIdx]);

                    if (currentDistance < minDistance)
                        minDistance = currentDistance;
                }

                accumulatedDistances += minDistance * minDistance;
                accDistances[pointIdx] = accumulatedDistances;
            }

            var targetPoint = T.CreateTruncating(random.NextSingle()) * accumulatedDistances;

            for (var pointIdx = 0; pointIdx < volume.Length; pointIdx++)
            {
                if (accDistances[pointIdx] >= targetPoint)
                {
                    clusters[i] = new KmCluster<T>(volume[pointIdx]);
                    break;
                }
            }
        }

        return clusters;
    }
}
