using FluentAssertions;
using KMeans.DistanceEstimators;
using Xunit;

namespace KMeans.Tests;

public sealed class ChebyshevDistanceTests
{
    // Lengths below and above Vector<double>.Count cover the scalar and the SIMD paths;
    // the maximum is placed both in the vectorized part and in the scalar tail.
    public static TheoryData<double[], double[], double> Cases => new()
    {
        { [1, 1], [10, 4], 9 },
        { [1, 2, 3], [1, 2, 3], 0 },
        { [-1, 2, -3, 4, -5, 6, -7, 8, -9], [0, 0, 0, 0, 0, 0, 0, 0, 0], 9 },
        { [-10, 1, 1, 1, 1, 1, 1, 1, 1], [0, 0, 0, 0, 0, 0, 0, 0, 0], 10 },
    };

    [Theory(DisplayName = "Should estimate the Chebyshev distance.")]
    [Trait("Category", "Unit")]
    [MemberData(nameof(Cases))]
    public void ShouldEstimate(double[] a, double[] b, double expected)
    {
        // Arrange
        var distance = new ChebyshevDistance<double>();

        // Act
        var actual = distance.Estimate(a, b);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory(DisplayName = "Should compare with the Chebyshev distance.")]
    [Trait("Category", "Unit")]
    [MemberData(nameof(Cases))]
    public void ShouldCompare(double[] a, double[] b, double expected)
    {
        // Arrange
        var distance = new ChebyshevDistance<double>();

        // Act
        var actual = distance.Compare(a, b);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact(DisplayName = "Should report the Chebyshev distance as a metric.")]
    [Trait("Category", "Unit")]
    public void ShouldBeMetric()
    {
        // Arrange
        var distance = new ChebyshevDistance<double>();

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
        var distance = new ChebyshevDistance<double>();
        double[] a = [1, 2, 3, 4];
        double[] b = [10, 10];

        // Act
        var exception = Record.Exception(() => distance.Estimate(a, b));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentOutOfRangeException>();
    }
}
