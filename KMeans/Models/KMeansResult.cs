using System;
using System.Numerics;

namespace KMeans.Models;

/// <summary>
/// Result of the K-Means clustering.
/// </summary>
public sealed class KMeansResult<T>
    where T : unmanaged, INumber<T>
{
    internal KMeansResult(
        T[] centroids,
        int dimension,
        int[] labels,
        long[] weights,
        int iterations)
    {
        Centroids = centroids;
        Dimension = dimension;
        Labels = labels;
        Weights = weights;
        Iterations = iterations;
    }

    /// <summary>
    /// Dimension of the vectors.
    /// </summary>
    public int Dimension { get; }

    /// <summary>
    /// Number of clusters.
    /// </summary>
    public int ClusterCount => Weights.Length;

    /// <summary>
    /// Centroids, row-major: <see cref="ClusterCount"/> × <see cref="Dimension"/>.
    /// </summary>
    public T[] Centroids { get; }

    /// <summary>
    /// Cluster index of every input point.
    /// </summary>
    public int[] Labels { get; }

    /// <summary>
    /// Number of points (sum of the point weights) in every cluster.
    /// </summary>
    public long[] Weights { get; }

    /// <summary>
    /// Number of performed iterations.
    /// </summary>
    public int Iterations { get; }

    /// <summary>
    /// Centroid of the cluster <paramref name="cluster"/>.
    /// </summary>
    public ReadOnlySpan<T> Centroid(int cluster) =>
        Centroids.AsSpan(cluster * Dimension, Dimension);
}
