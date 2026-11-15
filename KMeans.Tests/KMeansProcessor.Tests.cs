using FluentAssertions;
using KMeans.DistanceEstimators;
using KMeans.Initializers;
using KMeans.Models;
using Xunit;

namespace KMeans.Tests;

public sealed class KMeansProcessorTests
{
    [Fact(DisplayName = "Should find well separated clusters.")]
    [Trait("Category", "Unit")]
    public void ShouldFindSeparatedClusters()
    {
        // Arrange
        var processor = new KMeansProcessor<double, EuclideanDistance<double>>(
            new KppInitializer<double>(),
            options: new() { Seed = 1, MaxIterations = 100 });
        double[] centers = [0, 0, 100, 0, 0, 100];
        var points = new double[300 * 2];
        for (var i = 0; i < 300; i++)
        {
            var cloud = i / 100;
            points[i * 2] = centers[cloud * 2] + (i % 10) * 0.1;
            points[i * 2 + 1] = centers[cloud * 2 + 1] + (i / 10 % 10) * 0.1;
        }

        (double, double)[] expectedCentroids = [(0.45, 0.45), (0.45, 100.45), (100.45, 0.45)];
        long[] expectedWeights = [100, 100, 100];

        // Act
        var result = processor.Compute(points, 2, 3);
        var actualCentroids = Enumerable.Range(0, 3)
            .Select(c => (
                Math.Round(result.Centroids[c * 2], 9),
                Math.Round(result.Centroids[c * 2 + 1], 9)))
            .Order()
            .ToArray();

        // Assert
        actualCentroids.Should().Equal(expectedCentroids);
        result.Weights.Should().Equal(expectedWeights);
        result.Iterations.Should().BeLessThan(100);
    }

    [Fact(DisplayName = "Should assign every point when the ranges do not divide the points evenly.")]
    [Trait("Category", "Unit")]
    public void ShouldAssignEveryPoint()
    {
        // Arrange
        var processor = new KMeansProcessor<double, EuclideanDistance<double>>(
            new KppInitializer<double>(),
            options: new() { Seed = 1, ThreadCount = 3 });
        var random = new Random(7);
        var points = new double[10_007 * 2];
        for (var i = 0; i < points.Length; i++)
        {
            points[i] = random.NextDouble();
        }

        // Act
        var result = processor.Compute(points, 2, 5);
        var total = result.Weights.Sum();

        // Assert
        total.Should().Be(10_007);
        result.Labels.Should().NotContain(-1);
    }

    [Fact(DisplayName = "Should keep an empty cluster without an exception.")]
    [Trait("Category", "Unit")]
    public void ShouldKeepEmptyCluster()
    {
        // Arrange
        var processor = new KMeansProcessor<double, EuclideanDistance<double>>(
            new KppInitializer<double>(),
            options: new() { Seed = 1 });
        double[] points = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10];
        KMeansResult<double>? result = null;

        // Act
        var exception = Record.Exception(() => result = processor.Compute(points, 2, 3));

        // Assert
        exception.Should().BeNull();
        result!.Weights.Should().Contain(0);
        result.Centroids.Should().NotContain(double.NaN);
    }

    [Fact(DisplayName = "Should treat weighted points as repeated points.")]
    [Trait("Category", "Unit")]
    public void ShouldComputeWeighted()
    {
        // Arrange
        var processor = new KMeansProcessor<double, EuclideanDistance<double>>(
            new KppInitializer<double>(),
            options: new() { Seed = 1 });
        var unique = new double[30];
        var weights = new int[30];
        var repeated = new List<double>();
        for (var i = 0; i < 30; i++)
        {
            unique[i] = i / 10 * 100 + i % 10 * 0.1;
            weights[i] = i % 10 + 1;
            repeated.AddRange(Enumerable.Repeat(unique[i], weights[i]));
        }

        double[] expectedCentroids = [0.6, 100.6, 200.6];
        long[] expectedWeights = [55, 55, 55];

        // Act
        var weighted = processor.Compute(unique, 1, 3, weights);
        var expanded = processor.Compute(repeated.ToArray(), 1, 3);
        var weightedCentroids =
            weighted.Centroids.Select(c => Math.Round(c, 9)).Order().ToArray();
        var expandedCentroids =
            expanded.Centroids.Select(c => Math.Round(c, 9)).Order().ToArray();

        // Assert
        weightedCentroids.Should().Equal(expectedCentroids);
        expandedCentroids.Should().Equal(expectedCentroids);
        weighted.Weights.Should().Equal(expectedWeights);
        expanded.Weights.Should().Equal(expectedWeights);
    }

    [Fact(DisplayName = "Should produce the same result for the same seed.")]
    [Trait("Category", "Unit")]
    public void ShouldBeDeterministic()
    {
        // Arrange
        var processor = new KMeansProcessor<double, EuclideanDistance<double>>(
            new KppInitializer<double>(),
            options: new() { Seed = 5 });
        var random = new Random(11);
        var points = new double[20_000 * 3];
        for (var i = 0; i < points.Length; i++)
        {
            points[i] = random.NextDouble() * 100;
        }

        // Act
        var first = processor.Compute(points, 3, 8);
        var second = processor.Compute(points, 3, 8);

        // Assert
        first.Centroids.Should().Equal(second.Centroids);
        first.Labels.Should().Equal(second.Labels);
    }

    [Fact(DisplayName = "Should cluster integer vectors.")]
    [Trait("Category", "Unit")]
    public void ShouldComputeIntegers()
    {
        // Arrange
        var processor = new KMeansProcessor<int, ManhattanDistance<int>>(
            new KmInitializer<int>(),
            options: new() { Seed = 1 });
        int[] points = [0, 1, 2, 100, 101, 102];
        int[] expectedCentroids = [1, 101];
        long[] expectedWeights = [3, 3];

        // Act
        var result = processor.Compute(points, 1, 2);
        var centroids = result.Centroids.Order().ToArray();
        var weights = result.Weights.Order().ToArray();

        // Assert
        centroids.Should().Equal(expectedCentroids);
        weights.Should().Equal(expectedWeights);
    }

    [Fact(DisplayName = "Should not compute with a zero dimension.")]
    [Trait("Category", "Unit")]
    public void ShouldNotComputeWithZeroDimension()
    {
        // Arrange
        var processor = new KMeansProcessor<double, EuclideanDistance<double>>(
            new KppInitializer<double>());
        double[] points = [1, 2, 3, 4];

        // Act
        var exception = Record.Exception(() => processor.Compute(points, 0, 1));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentOutOfRangeException>();
    }

    [Fact(DisplayName = "Should not compute when the length is not a multiple of the dimension.")]
    [Trait("Category", "Unit")]
    public void ShouldNotComputeWithPartialVector()
    {
        // Arrange
        var processor = new KMeansProcessor<double, EuclideanDistance<double>>(
            new KppInitializer<double>());
        double[] points = [1, 2, 3, 4, 5];

        // Act
        var exception = Record.Exception(() => processor.Compute(points, 2, 1));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentException>();
    }

    [Fact(DisplayName = "Should not compute more clusters than points.")]
    [Trait("Category", "Unit")]
    public void ShouldNotComputeWithTooManyClusters()
    {
        // Arrange
        var processor = new KMeansProcessor<double, EuclideanDistance<double>>(
            new KppInitializer<double>());
        double[] points = [1, 2, 3, 4];

        // Act
        var exception = Record.Exception(() => processor.Compute(points, 2, 3));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentOutOfRangeException>();
    }

    [Fact(DisplayName = "Should not compute when the weights do not match the points.")]
    [Trait("Category", "Unit")]
    public void ShouldNotComputeWithWrongWeightCount()
    {
        // Arrange
        var processor = new KMeansProcessor<double, EuclideanDistance<double>>(
            new KppInitializer<double>());
        double[] points = [1, 2, 3, 4];
        int[] weights = [1, 1, 1];

        // Act
        var exception = Record.Exception(() => processor.Compute(points, 2, 1, weights));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentException>();
    }

    [Fact(DisplayName = "Should not compute with a negative weight.")]
    [Trait("Category", "Unit")]
    public void ShouldNotComputeWithNegativeWeight()
    {
        // Arrange
        var processor = new KMeansProcessor<double, EuclideanDistance<double>>(
            new KppInitializer<double>());
        double[] points = [1, 2, 3, 4];
        int[] weights = [1, -1];

        // Act
        var exception = Record.Exception(() => processor.Compute(points, 2, 1, weights));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentOutOfRangeException>();
    }
}
