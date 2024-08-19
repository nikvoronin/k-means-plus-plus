using System.Numerics;
using KMeans.Models;

namespace KMeans;

public interface IDistanceEstimator<T>
    where T : INumber<T>
{
    T Estimate(VectorN<T> value1, VectorN<T> value2);
}
