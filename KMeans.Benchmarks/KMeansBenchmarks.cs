using BenchmarkDotNet.Attributes;
using KMeans.DistanceEstimators;
using KMeans.Initializers;
using KMeans.Models;

namespace KMeans.Benchmarks;

[MemoryDiagnoser]
public class KMeansBenchmarks
{
    private const int PointCount = 120_000;
    private const int ClusterCount = 10;
    private const int Iterations = 20;

    [Params(3, 16)]
    public int Dimension;

    private double[] _points = [];
    private KMeansProcessor<double, EuclideanDistance<double>> _processor = null!;
    private readonly KppInitializer<double> _initializer = new();
    private readonly double[] _centroids = new double[ClusterCount * 16];

    [GlobalSetup]
    public void Setup()
    {
        _points = Blobs.Generate(PointCount, Dimension, blobs: ClusterCount, seed: 42);

        _processor = new KMeansProcessor<double, EuclideanDistance<double>>(
            _initializer,
            options: new ProcessorOptions
            {
                MaxIterations = Iterations,
                ConvergenceEpsilon = 0,
                Seed = 1
            });
    }

    [Benchmark]
    public int Compute() =>
        _processor.Compute(_points, Dimension, ClusterCount)
        .Iterations;

    [Benchmark]
    public int KppInitialize()
    {
        _initializer.Initialize(
            _points, default, Dimension,
            _centroids.AsSpan(0, ClusterCount * Dimension),
            new EuclideanDistance<double>(),
            new Random(1));

        return _centroids.Length;
    }
}
