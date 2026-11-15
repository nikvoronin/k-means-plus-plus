namespace KMeans.Benchmarks;

/// <summary>
/// Deterministic synthetic data: Gaussian blobs in a row-major flat buffer.
/// </summary>
public static class Blobs
{
    public static double[] Generate(
        int count,
        int dimension,
        int blobs,
        int seed)
    {
        var random = new Random(seed);

        var centers = new double[blobs * dimension];
        for (var i = 0; i < centers.Length; i++)
        {
            centers[i] = random.NextDouble() * 100.0;
        }

        var points = new double[count * dimension];
        for (var p = 0; p < count; p++)
        {
            var blob = random.Next(blobs);
            for (var d = 0; d < dimension; d++)
            {
                points[p * dimension + d] =
                    centers[blob * dimension + d]
                    + NextGaussian(random) * 8.0;
            }
        }

        return points;
    }

    private static double NextGaussian(Random random)
    {
        // Box-Muller
        var u1 = 1.0 - random.NextDouble();
        var u2 = random.NextDouble();
        return
            Math.Sqrt(-2.0 * Math.Log(u1))
            * Math.Cos(2.0 * Math.PI * u2);
    }
}
