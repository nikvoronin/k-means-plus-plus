using System;
using System.Numerics;
using KMeans.Models;

namespace KMeans;

public interface IKmInitializer<T>
    where T : INumber<T>
{
    public KmCluster<T>[] Initialize(
        VectorN<T>[] volume, int numClusters,
        IDistanceEstimator<T> distanceEstimator,
        Random? random = null);
}
