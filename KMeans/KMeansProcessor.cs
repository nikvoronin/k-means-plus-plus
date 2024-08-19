using KMeans.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace KMeans;

/// <summary>
/// K-Means clustering algorithm.
/// </summary>
public class KMeansProcessor<T>(
    IKmInitializer<T> clusterInitializer,
    IDistanceEstimator<T> distanceEstimator )
    where T : INumber<T>
{
    /// <summary>
    /// Split array of volume into clusters.
    /// </summary>
    /// <param name="volume">Vector volume.</param>
    /// <param name="numClusters">Number of clusters to produce.</param>
    /// <returns>Array of clusters.</returns>
    public KmCluster<T>[] Compute( VectorN<T>[] volume, int numClusters )
    {
        var clusters =
            _clusterInitializer.Initialize( volume, numClusters, _distance );

        // TODO: add option for upper limit of iterations
        for (var iteration = 0; iteration < 1000; iteration++) {
            var newClusters =
                ArrangePointsParallel( volume, clusters );

            if (IsStable( clusters, newClusters ))
                return newClusters;

            clusters = newClusters;
        }

        return clusters;
    }

    /// <summary>
    /// Assign volume of the incoming array to the node clusters with the nearest centroid
    /// </summary>
    /// <param name="volume">
    /// Vector volume.
    /// </param>
    /// <param name="sourceClusters">
    /// Previous set of clusters.
    /// </param>
    /// <returns>
    /// Produced next set of clusters.
    /// </returns>
    private KmCluster<T>[] ArrangePointsParallel(
        VectorN<T>[] volume,
        KmCluster<T>[] sourceClusters )
    {
        var numClusters = sourceClusters.Length;

        var nextClusters = new KmCluster<T>[numClusters];
        for (var i = 0; i < numClusters; i++)
            nextClusters[i] = new KmCluster<T>();

        var numNodes = Environment.ProcessorCount; // TODO: limit threads number

        var nodes = new List<KmCluster<T>[]>();
        for (var k = 0; k < numNodes; k++) {
            var newClusters = new KmCluster<T>[nextClusters.Length];
            for (var i = 0; i < newClusters.Length; i++)
                newClusters[i] = new KmCluster<T>();

            nodes.Add( newClusters );
        }

        var centroids = new VectorN<T>[numNodes][];
        for (var nodeIx = 0; nodeIx < numNodes; nodeIx++) {
            centroids[nodeIx] = new VectorN<T>[sourceClusters.Length];
            for (int clusterIx = 0; clusterIx < sourceClusters.Length; clusterIx++) {
                centroids[nodeIx][clusterIx] =
                    sourceClusters[clusterIx].Centroid.Clone();
            }
        }

        Parallel.For( 0, numNodes, nodeIx => {
            var chunkLen = volume.Length / numNodes;
            var startIx = nodeIx * chunkLen;
            for (int px = startIx; px < startIx + chunkLen; px++) {
                var point = volume[px];

                // TODO? extract distance calculator into a class
                var minDistance =
                    _distance.Estimate(
                        centroids[nodeIx][0],
                        point );

                var bestClusterIx = 0;
                for (int ix = 1; ix < sourceClusters.Length; ix++) {
                    var distance =
                        _distance.Estimate(
                            centroids[nodeIx][ix],
                            point );

                    if (distance < minDistance) {
                        minDistance = distance;
                        bestClusterIx = ix;
                    }
                }

                nodes[nodeIx][bestClusterIx]
                    .AddPoint( point );
            }
        } );

        for (var nodeIx = 0; nodeIx < nodes.Count; nodeIx++) {
            for (var clusterIx = 0; clusterIx < nextClusters.Length; clusterIx++) {
                var sourceCluster = nodes[nodeIx][clusterIx];
                for (var px = 0; px < sourceCluster.Points.Count; px++)
                    nextClusters[clusterIx].AddPoint( sourceCluster.Points[px] );
            }
        }

        return nextClusters;
    }

    /// <summary>
    /// Verify if the clusterization has converged by comparing the clusters.
    /// </summary>
    /// <param name="sourceClusters">
    /// Previous set of clusters.
    /// </param>
    /// <param name="nextClusters">
    /// Next produced set of clusters.
    /// </param>
    /// <returns>
    /// <see langword="true"> if converged,
    /// <see langword="false"> otherwise.
    /// </returns>
    private bool IsStable(
        KmCluster<T>[] sourceClusters,
        KmCluster<T>[] nextClusters,
        float epsilon = .1f )
    {
        for (var i = 0; i < sourceClusters.Length; i++) {
            if (sourceClusters[i].Points.Count != nextClusters[i].Points.Count)
                return false;
        }

        var eps = T.CreateTruncating( epsilon );

        for (var i = 0; i < sourceClusters.Length; i++) {
            var distance =
                _distance.Estimate(
                    sourceClusters[i].Centroid,
                    nextClusters[i].Centroid );

            if (distance > eps) return false;
        }

        return true;
    }

    private readonly IKmInitializer<T> _clusterInitializer = clusterInitializer;
    private readonly IDistanceEstimator<T> _distance = distanceEstimator;
}
