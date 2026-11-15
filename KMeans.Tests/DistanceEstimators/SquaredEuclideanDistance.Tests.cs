using FluentAssertions;
using KMeans.DistanceEstimators;
using Xunit;

namespace KMeans.Tests;

public sealed class SquaredEuclideanDistanceTests
{
    // Lengths below and above Vector<double>.Count cover the scalar and the SIMD paths.
    public static TheoryData<double[], double[], double> Cases => new()
    {
        { [1, 1], [10, 10], 162 },
        { [1, 2, 3], [1, 2, 3], 0 },
        { [0, 1, 2, 3, 4, 5, 6, 7, 8], [0, 0, 0, 0, 0, 0, 0, 0, 0], 204 },
    };

    [Theory(DisplayName = "Should estimate the squared Euclidean distance.")]
    [Trait("Category", "Unit")]
    [MemberData(nameof(Cases))]
    public void ShouldEstimate(double[] a, double[] b, double expected)
    {
        // Arrange
        var distance = new SquaredEuclideanDistance<double>();

        // Act
        var actual = distance.Estimate(a, b);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory(DisplayName = "Should compare with the squared Euclidean distance.")]
    [Trait("Category", "Unit")]
    [MemberData(nameof(Cases))]
    public void ShouldCompare(double[] a, double[] b, double expected)
    {
        // Arrange
        var distance = new SquaredEuclideanDistance<double>();

        // Act
        var actual = distance.Compare(a, b);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact(DisplayName = "Should estimate integer vectors.")]
    [Trait("Category", "Unit")]
    public void ShouldEstimateIntegers()
    {
        // Arrange
        var distance = new SquaredEuclideanDistance<int>();
        int[] a = [1, 1, 1, 1, 1, 1, 1, 1, 1, 2];
        int[] b = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        // Act
        var actual = distance.Estimate(a, b);

        // Assert
        actual.Should().Be(13);
    }

    [Fact(DisplayName = "Should not report the squared Euclidean distance as a metric.")]
    [Trait("Category", "Unit")]
    public void ShouldNotBeMetric()
    {
        // Arrange
        var distance = new SquaredEuclideanDistance<double>();

        // Act
        var actual = distance.IsMetric;

        // Assert
        actual.Should().BeFalse();
    }

    [Fact(DisplayName = "Should not estimate vectors of different dimensions.")]
    [Trait("Category", "Unit")]
    public void ShouldNotEstimateOnDimensionsMismatch()
    {
        // Arrange
        var distance = new SquaredEuclideanDistance<double>();
        double[] a = [1, 2, 3, 4];
        double[] b = [10, 10];

        // Act
        var exception = Record.Exception(() => distance.Estimate(a, b));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentOutOfRangeException>();
    }
}
