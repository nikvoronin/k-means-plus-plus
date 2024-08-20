using System;

namespace KMeans.Models;

public sealed class ProcessorOptions
{
    public int ThreadCount { get; init; } = Environment.ProcessorCount;
    public int MaxIterations { get; init; } = 1000;
    public double ConvergenceEpsilon { get; init; } = .1;
}
