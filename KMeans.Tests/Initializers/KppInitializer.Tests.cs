using FluentAssertions;
using KMeans.DistanceEstimators;
using KMeans.Initializers;
using Xunit;

namespace KMeans.Tests;

public sealed class KppInitializerTests
{
    [Fact(DisplayName = "Should choose distinct points as centroids.")]
    [Trait("Category", "Unit")]
    public void ShouldChooseDistinctCentroids()
    {
        // Arrange
        var initializer = new KppInitializer<double>();
        double[] points = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
        var centroids = new double[10];
        double[] expected = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        initializer.Initialize(
            points,
            default,
            1,
            centroids,
            new EuclideanDistance<double>(),
            new Random(3));

        // Assert
        centroids.Should().BeEquivalentTo(expected);
    }

    [Fact(DisplayName = "Should prefer a far point by the squared distance.")]
    [Trait("Category", "Unit")]
    public void ShouldPreferFarPoint()
    {
        // Arrange
        var initializer = new KppInitializer<double>();
        var points = new double[100];
        for (var i = 0; i < 99; i++)
            points[i] = i * 0.01;
        points[99] = 1000;
        var centroids = new double[2];

        // Act
        initializer.Initialize(
            points,
            default,
            1,
            centroids,
            new EuclideanDistance<double>(),
            new Random(3));

        // Assert
        centroids.Should().Contain(1000);
    }

    [Fact(DisplayName = "Should never choose a point with zero weight.")]
    [Trait("Category", "Unit")]
    public void ShouldSkipZeroWeights()
    {
        // Arrange
        var initializer = new KppInitializer<double>();
        double[] points = [0, 1, 2, 3, 4, 5];
        int[] weights = [0, 1, 0, 1, 0, 1];
        var centroids = new double[3];
        double[] expected = [1, 3, 5];

        // Act
        initializer.Initialize(
            points,
            weights,
            1,
            centroids,
            new EuclideanDistance<double>(),
            new Random(3));

        // Assert
        centroids.Should().BeEquivalentTo(expected);
    }
}
