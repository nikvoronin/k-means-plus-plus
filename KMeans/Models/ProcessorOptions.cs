using System;

namespace KMeans.Models;

/// <summary>
/// Options of the <see cref="KMeansProcessor{T, TDistance}"/>.
/// </summary>
public sealed class ProcessorOptions
{
    /// <summary>
    /// Maximum number of threads;
    /// <see cref="Environment.ProcessorCount"/> by default.
    /// </summary>
    public int ThreadCount { get; init; } = Environment.ProcessorCount;

    /// <summary>
    /// Stop after this number of iterations even if not converged.
    /// </summary>
    public int MaxIterations { get; init; } = 1000;

    /// <summary>
    /// Stop when no centroid moved further than this distance.
    /// </summary>
    public double ConvergenceEpsilon { get; init; } = .1;

    /// <summary>
    /// Seed of the initializer randomizer; 
    /// <see langword="null"/> means <see cref="Random.Shared"/>.
    /// </summary>
    public int? Seed { get; init; }

    /// <summary>
    /// Skip distance computations with the Hamerly bounds when the distance is a metric
    /// (<see cref="IDistanceEstimator{T}.IsMetric"/>). The result is the same as without them.
    /// </summary>
    public bool UseHamerlyBounds { get; init; } = true;
}
