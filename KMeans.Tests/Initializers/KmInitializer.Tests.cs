using FluentAssertions;
using KMeans.DistanceEstimators;
using KMeans.Initializers;
using Xunit;

namespace KMeans.Tests;

public sealed class KmInitializerTests
{
    [Fact(DisplayName = "Should choose distinct points as centroids.")]
    [Trait("Category", "Unit")]
    public void ShouldChooseDistinctCentroids()
    {
        // Arrange
        var initializer = new KmInitializer<double>();
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
}
