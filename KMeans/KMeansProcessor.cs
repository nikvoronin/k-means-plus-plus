using KMeans.Models;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace KMeans;

/// <summary>
/// K-Means clustering algorithm (Lloyd iterations).
/// </summary>
/// <remarks>
/// <typeparamref name="TDistance"/> is a <see langword="struct"/>, 
/// so the JIT specializes the hot loop for it and inlines the distance computation.
/// </remarks>
public sealed class KMeansProcessor<T, TDistance>(
    IKmInitializer<T> clusterInitializer,
    TDistance distanceEstimator = default,
    ProcessorOptions? options = null)
    where T : unmanaged, INumber<T>
    where TDistance : struct, IDistanceEstimator<T>
{
    /// <summary>
    /// Split points into clusters.
    /// </summary>
    /// <param name="points">
    /// Points, row-major: n × <paramref name="dimension"/>.
    /// </param>
    /// <param name="dimension">
    /// Dimension of the vectors.
    /// </param>
    /// <param name="clusterCount">
    /// Number of clusters to produce.
    /// </param>
    /// <param name="weights">
    /// Optional non-negative weight of every point (for example, the number of duplicates).
    /// Empty means every point has weight 1.
    /// </param>
    public KMeansResult<T> Compute(
        ReadOnlyMemory<T> points,
        int dimension,
        int clusterCount,
        ReadOnlyMemory<int> weights = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimension);

        if (points.Length % dimension != 0)
        {
            throw new ArgumentException(
                $"Length {points.Length} is not a multiple of the dimension {dimension}.",
                nameof(points));
        }

        var n = points.Length / dimension;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(clusterCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(clusterCount, n);

        if (!weights.IsEmpty)
        {
            if (weights.Length != n)
            {
                throw new ArgumentException(
                    $"Expected {n} weights, got {weights.Length}.",
                    nameof(weights));
            }

            foreach (var w in weights.Span)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(w, nameof(weights));
            }
        }

        var random = _options.Seed is { } seed
            ? new Random(seed)
            : Random.Shared;

        var centroids = new T[clusterCount * dimension];
        _clusterInitializer.Initialize(
            points, weights, dimension, centroids, _distance, random);

        var labels = new int[n];
        Array.Fill(labels, -1);

        var sums = new T[centroids.Length];
        var counts = new long[clusterCount];
        var nextCentroid = new T[dimension];
        var eps = T.CreateSaturating(_options.ConvergenceEpsilon);

        var threadCount = Math.Max(1, _options.ThreadCount);
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = threadCount
        };
        var distance = _distance;

        // Fixed ranges with their own accumulators, merged in order:
        // no allocations or locks per iteration, and the floating-point sums
        // do not depend on thread scheduling.
        // Small ranges balance the load between fast and slow cores (hybrid CPUs).
        var rangeSize = Math.Max(128, (n + threadCount * 32 - 1) / (threadCount * 32));
        var rangeCount = (n + rangeSize - 1) / rangeSize;
        var accumulators = new Accumulator[rangeCount];

        var iteration = 0;
        while (iteration < _options.MaxIterations)
        {
            iteration++;

            Parallel.For(0, rangeCount, parallelOptions, r =>
            {
                // Allocated lazily by the worker thread: its own allocation context
                // keeps the hot accumulators of different threads on different cache lines.
                var acc = accumulators[r] ??= new Accumulator(clusterCount, dimension);
                acc.Reset();
                Assign(
                    distance,
                    points.Span, weights.Span, dimension,
                    centroids, labels,
                    r * rangeSize, Math.Min(n, (r + 1) * rangeSize),
                    acc);
            });

            Array.Clear(sums);
            Array.Clear(counts);
            long changed = 0;
            foreach (var acc in accumulators)
            {
                for (var i = 0; i < sums.Length; i++)
                    sums[i] += acc.Sums[i];

                for (var c = 0; c < counts.Length; c++)
                    counts[c] += acc.Counts[c];

                changed += acc.Changed;
            }

            var shift = T.Zero;
            for (var c = 0; c < clusterCount; c++)
            {
                // An empty cluster keeps its previous centroid.
                if (counts[c] == 0) continue;

                var count = T.CreateTruncating(counts[c]);
                var sum = sums.AsSpan(c * dimension, dimension);
                for (var d = 0; d < dimension; d++)
                    nextCentroid[d] = sum[d] / count;

                var centroid = centroids.AsSpan(c * dimension, dimension);
                shift = T.Max(shift, distance.Estimate(centroid, nextCentroid));

                nextCentroid.CopyTo(centroid);
            }

            if (changed == 0 || shift <= eps) break;
        }

        return new KMeansResult<T>(centroids, dimension, labels, counts, iteration);
    }

    /// <summary>
    /// Assign the points <c>[from, to)</c> to the nearest centroid
    /// and accumulate their sums into the thread-local <paramref name="acc"/>.
    /// </summary>
    private static void Assign(
        TDistance distance,
        ReadOnlySpan<T> points,
        ReadOnlySpan<int> weights,
        int dimension,
        ReadOnlySpan<T> centroids,
        Span<int> labels,
        int from, int to,
        Accumulator acc)
    {
        var clusterCount = centroids.Length / dimension;
        var sums = acc.Sums.AsSpan();
        var counts = acc.Counts;
        long changed = 0;

        for (var i = from; i < to; i++)
        {
            var point = points.Slice(i * dimension, dimension);

            var best = 0;
            var bestDistance = distance.Compare(centroids[..dimension], point);
            for (var c = 1; c < clusterCount; c++)
            {
                var d = distance.Compare(
                    centroids.Slice(c * dimension, dimension), point);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = c;
                }
            }

            if (labels[i] != best)
            {
                labels[i] = best;
                changed++;
            }

            var sum = sums.Slice(best * dimension, dimension);
            if (weights.IsEmpty)
            {
                counts[best]++;

                for (var d = 0; d < sum.Length; d++)
                    sum[d] += point[d];
            }
            else
            {
                var w = weights[i];
                counts[best] += w;
                var tw = T.CreateTruncating(w);

                for (var d = 0; d < sum.Length; d++)
                    sum[d] += tw * point[d];
            }
        }

        acc.Changed += changed;
    }

    private sealed class Accumulator(int clusterCount, int dimension)
    {
        public readonly T[] Sums = new T[clusterCount * dimension];
        public readonly long[] Counts = new long[clusterCount];
        public long Changed;

        public void Reset()
        {
            Array.Clear(Sums);
            Array.Clear(Counts);
            Changed = 0;
        }
    }

    private readonly IKmInitializer<T> _clusterInitializer = clusterInitializer;
    private readonly TDistance _distance = distanceEstimator;
    private readonly ProcessorOptions _options = options ?? new();
}
