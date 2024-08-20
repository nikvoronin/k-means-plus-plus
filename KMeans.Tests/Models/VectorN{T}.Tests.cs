using FluentAssertions;
using KMeans.Models;

namespace KMeans.Tests.Models
{
    public class VectorNTests
    {
        [Fact]
        public void CanCreateNumberTypes()
        {
            // Arrange
            var arrF = new float[] { -1f, 0f, 2f, -3f, 4f, 5f };
            var arrD = new double[] { -1.0, 2.0, 0.0, 4.0, 5.0 };
            var arrInt = new int[] { -1, 2, 0, 4, 5 };
            var arrB = new byte[] { 1, 2, 0, 4, 5 };
            var arrDec = new decimal[] { -1m, 2m, 0m, 4m, 5m };

            // Act
            var vectorF = new VectorN<float>(arrF);
            var vectorD = new VectorN<double>(arrD);
            var vectorInt = new VectorN<int>(arrInt);
            var vectorB = new VectorN<byte>(arrB);
            var vectorDec = new VectorN<decimal>(arrDec);

            // Assert
            vectorF.Components.Should().ContainInConsecutiveOrder(arrF);
            vectorD.Components.Should().ContainInConsecutiveOrder(arrD);
            vectorInt.Components.Should().ContainInConsecutiveOrder(arrInt);
            vectorB.Components.Should().ContainInConsecutiveOrder(arrB);
            vectorDec.Components.Should().ContainInConsecutiveOrder(arrDec);
        }
    }
}