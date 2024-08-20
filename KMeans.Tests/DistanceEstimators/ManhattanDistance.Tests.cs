using FluentAssertions;
using KMeans.DistanceEstimators;
using KMeans.Models;

namespace KMeans.Tests.Models
{
    public class ManhattanDistanceTests
    {
        [Fact]
        public void CanEstimate()
        {
            // Arrange
            var distance = new ManhattanDistance<double>();
            var a = new VectorN<double>( 1, 1 );
            var b = new VectorN<double>( 10, 10 );
            var expected = 18;

            // Act
            var actual = distance.Estimate( a, b );

            // Assert
            actual.Should().Be( expected );
        }

        [Fact]
        public void CanNotEstimateOnDimensionsMismatch()
        {
            // Arrange
            var distance = new EuclideanDistance<double>();
            var a = new VectorN<double>( 1, 2, 3, 4 );
            var b = new VectorN<double>( 10, 10 );

            // Act
            var exception = Record.Exception( () =>
                distance.Estimate( a, b ) );

            // Assert
            exception.Should().NotBeNull()
                .And.BeOfType<ArgumentOutOfRangeException>();
        }
    }
}