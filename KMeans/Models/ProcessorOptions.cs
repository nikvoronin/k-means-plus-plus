using System;

namespace KMeans.Models;

public sealed class ProcessorOptions
{
    public int ThreadCount { get; init; } = Environment.ProcessorCount;
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
}
