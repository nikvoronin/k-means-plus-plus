using System;
using System.Numerics;
using System.Threading.Tasks;

namespace KMeans.Initializers;

/// <summary>
/// K-Means++ initializer: every next centroid is a point chosen
/// with probability proportional to w·D², where D is the distance to the nearest chosen centroid.
/// </summary>
public sealed class KppInitializer<T> : IKmInitializer<T>
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
        var rangeCount = Math.Max(1, Math.Min(Environment.ProcessorCount * 4, n / 1024));
        var rangeSize = (n + rangeCount - 1) / rangeCount;

        // Weighted squared distance to the nearest chosen centroid.
        // Before the first pick it holds the weights,
        // so the first centroid is chosen by weight.
        var scores = new double[n];

        if (weights.IsEmpty)
        {
            Array.Fill(scores, 1.0);
        }
        else
        {
            var ws = weights.Span;

            for (var i = 0; i < n; i++)
            {
                scores[i] = ws[i];
            }
        }

        var center = new T[dimension];

        for (var c = 0; c < clusterCount; c++)
        {
            var picked = Pick(scores, random);
            points.Span.Slice(picked * dimension, dimension)
                .CopyTo(center);
            center.CopyTo(centroids.Slice(c * dimension, dimension));

            if (c == clusterCount - 1)
                break;

            var isFirst = c == 0;
            // Parallel.For over fixed ranges: much cheaper here than ForEach over a range Partitioner.
            Parallel.For(0, rangeCount, r =>
            {
                var pts = points.Span;
                var ws = weights.Span;
                var to = Math.Min(n, (r + 1) * rangeSize);

                for (var i = r * rangeSize; i < to; i++)
                {
                    var d = double.CreateTruncating(
                        distance.Estimate(center, pts.Slice(i * dimension, dimension)));

                    var score = (ws.IsEmpty ? 1.0 : ws[i]) * d * d;
                    scores[i] = isFirst ? score : Math.Min(scores[i], score);
                }
            });
        }
    }

    /// <summary>
    /// Index chosen with probability proportional to its score.
    /// </summary>
    private static int Pick(double[] scores, Random random)
    {
        var total = 0.0;

        foreach (var s in scores)
        {
            total += s;
        }

        // Every point coincides with a chosen centroid (or has zero weight).
        if (!(total > 0)
            || !double.IsFinite(total))
        {
            return random.Next(scores.Length);
        }

        var target = random.NextDouble() * total;
        var acc = 0.0;
        var last = 0;

        for (var i = 0; i < scores.Length; i++)
        {
            if (scores[i] <= 0)
                continue;

            acc += scores[i];
            last = i;

            if (acc > target)
            {
                return i;
            }
        }

        // Rounding: the accumulated sum fell slightly short of the target.
        return last;
    }
}
