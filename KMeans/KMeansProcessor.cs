using KMeans.Models;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace KMeans;

/// <summary>
/// K-Means clustering algorithm: Lloyd iterations, accelerated with the Hamerly bounds
/// when the distance is a metric.
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
        var shifts = new T[clusterCount];
        var nextCentroid = new T[dimension];
        var eps = T.CreateSaturating(_options.ConvergenceEpsilon);

        var threadCount = Math.Max(1, _options.ThreadCount);
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = threadCount
        };
        var distance = _distance;

        // The bounds rely on the triangle inequality
        // and need a second centroid for the lower bound.
        var bounds =
            _options.UseHamerlyBounds 
            && distance.IsMetric 
            && clusterCount >= 2
                ? new HamerlyBounds(n, clusterCount)
                : null;

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

            bounds?.UpdateHalfGaps(distance, centroids, dimension);

            Parallel.For(0, rangeCount, parallelOptions, r =>
            {
                // Allocated lazily by the worker thread: its own allocation context
                // keeps the hot accumulators of different threads on different cache lines.
                var acc = accumulators[r] ??= new Accumulator(clusterCount, dimension);
                acc.Reset();

                var from = r * rangeSize;
                var to = Math.Min(n, (r + 1) * rangeSize);

                if (bounds is null)
                {
                    Assign(
                        distance,
                        points.Span, weights.Span, dimension,
                        centroids, labels,
                        from, to,
                        acc);
                }
                else
                {
                    AssignHamerly(
                        distance,
                        points.Span, weights.Span, dimension,
                        centroids, labels, bounds,
                        from, to,
                        acc);
                }
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

            var maxShift = T.Zero;
            for (var c = 0; c < clusterCount; c++)
            {
                // An empty cluster keeps its previous centroid.
                shifts[c] = T.Zero;
                if (counts[c] == 0) continue;

                var count = T.CreateTruncating(counts[c]);
                var sum = sums.AsSpan(c * dimension, dimension);
                for (var d = 0; d < dimension; d++)
                    nextCentroid[d] = sum[d] / count;

                var centroid = centroids.AsSpan(c * dimension, dimension);
                shifts[c] = distance.Estimate(centroid, nextCentroid);
                maxShift = T.Max(maxShift, shifts[c]);

                nextCentroid.CopyTo(centroid);
            }

            bounds?.SetShifts(shifts);

            if (changed == 0 || maxShift <= eps) break;
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

            AddPoint(acc, weights, i, best, point);
        }

        acc.Changed += changed;
    }

    /// <summary>
    /// Same as <see cref="Assign"/>, but skips the points whose bounds prove
    /// that the nearest centroid has not changed (Hamerly, 2010).
    /// </summary>
    private static void AssignHamerly(
        TDistance distance,
        ReadOnlySpan<T> points,
        ReadOnlySpan<int> weights,
        int dimension,
        ReadOnlySpan<T> centroids,
        Span<int> labels,
        HamerlyBounds bounds,
        int from, int to,
        Accumulator acc)
    {
        var upper = bounds.Upper;
        var lower = bounds.Lower;
        var shifts = bounds.Shifts;
        var halfGaps = bounds.HalfGaps;
        var maxShift = bounds.MaxShift;
        var maxShiftCluster = bounds.MaxShiftCluster;
        var secondShift = bounds.SecondShift;
        long changed = 0;

        for (var i = from; i < to; i++)
        {
            var point = points.Slice(i * dimension, dimension);
            var label = labels[i];

            if (label < 0)
            {
                // First iteration: no bounds yet.
                label = FindNearest(
                    distance, 
                    centroids, 
                    dimension, 
                    point, 
                    out upper[i], 
                    out lower[i]);
            }
            else
            {
                // Move the bounds by the last centroid shifts.
                var u = upper[i] + shifts[label];
                var l = lower[i] - (label == maxShiftCluster ? secondShift : maxShift);
                var limit = T.Max(halfGaps[label], l);

                if (u > limit)
                {
                    // Tighten the upper bound and check again.
                    u = distance.Estimate(
                        centroids.Slice(label * dimension, dimension), 
                        point);

                    if (u > limit)
                    {
                        label = FindNearest(
                            distance, 
                            centroids, 
                            dimension, 
                            point, 
                            out u, 
                            out l);
                    }
                }

                upper[i] = u;
                lower[i] = l;
            }

            if (labels[i] != label)
            {
                labels[i] = label;
                changed++;
            }

            AddPoint(acc, weights, i, label, point);
        }

        acc.Changed += changed;
    }

    /// <summary>
    /// Nearest centroid by <see cref="IDistanceEstimator{T}.Compare"/>
    /// (ties go to the lower index, as in <see cref="Assign"/>),
    /// with the distances to the nearest and to the second nearest centroid.
    /// </summary>
    private static int FindNearest(
        TDistance distance,
        ReadOnlySpan<T> centroids,
        int dimension,
        ReadOnlySpan<T> point,
        out T nearestDistance,
        out T secondDistance)
    {
        var clusterCount = centroids.Length / dimension;

        var best = 0;
        var second = 1;
        var bestValue = distance.Compare(
            centroids[..dimension], 
            point);
        var secondValue = distance.Compare(centroids
            .Slice(dimension, dimension), 
            point);

        if (secondValue < bestValue)
        {
            (best, second) = (second, best);
            (bestValue, secondValue) = (secondValue, bestValue);
        }

        for (var c = 2; c < clusterCount; c++)
        {
            var value = distance.Compare(
                centroids.Slice(c * dimension, dimension), 
                point);

            if (value < bestValue)
            {
                second = best;
                secondValue = bestValue;
                best = c;
                bestValue = value;
            }
            else if (value < secondValue)
            {
                second = c;
                secondValue = value;
            }
        }

        nearestDistance = distance.Estimate(
            centroids.Slice(best * dimension, dimension), 
            point);
        secondDistance = distance.Estimate(
            centroids.Slice(second * dimension, dimension), 
            point);

        return best;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddPoint(
        Accumulator acc,
        ReadOnlySpan<int> weights,
        int index,
        int label,
        ReadOnlySpan<T> point)
    {
        var sum = acc.Sums.AsSpan(label * point.Length, point.Length);

        if (weights.IsEmpty)
        {
            acc.Counts[label]++;

            for (var d = 0; d < sum.Length; d++)
            {
                sum[d] += point[d];
            }
        }
        else
        {
            var w = weights[index];
            acc.Counts[label] += w;
            var tw = T.CreateTruncating(w);

            for (var d = 0; d < sum.Length; d++)
            {
                sum[d] += tw * point[d];
            }
        }
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

    /// <summary>
    /// State of the Hamerly algorithm.
    /// </summary>
    private sealed class HamerlyBounds(int pointCount, int clusterCount)
    {
        /// <summary>
        /// Upper bound of the distance from a point to its centroid.
        /// </summary>
        public readonly T[] Upper = new T[pointCount];

        /// <summary>
        /// Lower bound of the distance from a point to any other centroid.
        /// </summary>
        public readonly T[] Lower = new T[pointCount];

        /// <summary>
        /// How far every centroid moved in the last update.
        /// </summary>
        public readonly T[] Shifts = new T[clusterCount];

        /// <summary>
        /// Half of the distance from a centroid to the nearest other centroid:
        /// a point closer than that to its centroid cannot change the cluster.
        /// </summary>
        public readonly T[] HalfGaps = new T[clusterCount];

        public T MaxShift;
        public int MaxShiftCluster;
        public T SecondShift;

        public void UpdateHalfGaps(
            TDistance distance, 
            ReadOnlySpan<T> centroids, 
            int dimension)
        {
            var clusterCount = HalfGaps.Length;

            for (var c = 0; c < clusterCount; c++)
            {
                var other = (c + 1) % clusterCount;

                HalfGaps[c] = distance.Estimate(
                    centroids.Slice(c * dimension, dimension),
                    centroids.Slice(other * dimension, dimension));
            }

            for (var c = 0; c < clusterCount; c++)
            {
                for (var other = c + 1; other < clusterCount; other++)
                {
                    var d = distance.Estimate(
                        centroids.Slice(c * dimension, dimension),
                        centroids.Slice(other * dimension, dimension));

                    HalfGaps[c] = T.Min(HalfGaps[c], d);
                    HalfGaps[other] = T.Min(HalfGaps[other], d);
                }
            }

            // Integer division rounds down: the bound stays conservative.
            var two = T.CreateTruncating(2);

            for (var c = 0; c < clusterCount; c++)
            {
                HalfGaps[c] /= two;
            }
        }

        public void SetShifts(ReadOnlySpan<T> shifts)
        {
            shifts.CopyTo(Shifts);

            MaxShift = T.Zero;
            MaxShiftCluster = 0;
            SecondShift = T.Zero;

            for (var c = 0; c < shifts.Length; c++)
            {
                if (shifts[c] > MaxShift)
                {
                    SecondShift = MaxShift;
                    MaxShift = shifts[c];
                    MaxShiftCluster = c;
                }
                else if (shifts[c] > SecondShift)
                {
                    SecondShift = shifts[c];
                }
            }
        }
    }

    private readonly IKmInitializer<T> _clusterInitializer = clusterInitializer;
    private readonly TDistance _distance = distanceEstimator;
    private readonly ProcessorOptions _options = options ?? new();
}
