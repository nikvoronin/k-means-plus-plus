using FluentAssertions;
using KMeans.DistanceEstimators;
using Xunit;

namespace KMeans.Tests;

public sealed class EuclideanDistanceTests
{
    // Lengths below and above Vector<double>.Count cover the scalar and the SIMD paths.
    public static TheoryData<double[], double[], double> Cases => new()
    {
        { [0, 0], [3, 4], 5 },
        { [1, 2, 3], [1, 2, 3], 0 },
        { [1, 1, 1, 1, 1, 1, 1, 1, 1], [0, 0, 0, 0, 0, 0, 0, 0, 0], 3 },
        {
            [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            8
        },
    };

    [Theory(DisplayName = "Should estimate the Euclidean distance.")]
    [Trait("Category", "Unit")]
    [MemberData(nameof(Cases))]
    public void ShouldEstimate(double[] a, double[] b, double expected)
    {
        // Arrange
        var distance = new EuclideanDistance<double>();

        // Act
        var actual = distance.Estimate(a, b);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact(DisplayName = "Should compare with the squared distance.")]
    [Trait("Category", "Unit")]
    public void ShouldCompareWithoutRoot()
    {
        // Arrange
        var distance = new EuclideanDistance<double>();
        double[] a = [0, 0];
        double[] b = [3, 4];

        // Act
        var actual = distance.Compare(a, b);

        // Assert
        actual.Should().Be(25);
    }

    [Fact(DisplayName = "Should report the Euclidean distance as a metric.")]
    [Trait("Category", "Unit")]
    public void ShouldBeMetric()
    {
        // Arrange
        var distance = new EuclideanDistance<double>();

        // Act
        var actual = distance.IsMetric;

        // Assert
        actual.Should().BeTrue();
    }

    [Fact(DisplayName = "Should not estimate vectors of different dimensions.")]
    [Trait("Category", "Unit")]
    public void ShouldNotEstimateOnDimensionsMismatch()
    {
        // Arrange
        var distance = new EuclideanDistance<double>();
        double[] a = [1, 2, 3, 4];
        double[] b = [10, 10];

        // Act
        var exception = Record.Exception(() => distance.Estimate(a, b));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentOutOfRangeException>();
    }
}
