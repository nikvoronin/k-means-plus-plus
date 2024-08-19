using System;
using System.Numerics;
using KMeans.Models;

namespace KMeans.DistanceEstimators;

public class ChebyshevDistance<T> : IDistanceEstimator<T>
    where T : INumber<T>
{
    public T Estimate(VectorN<T> a, VectorN<T> b)
    {
        if (a.Dimension != b.Dimension)
        {
            throw new ArgumentOutOfRangeException(
                $"Vectors dimension mismatch: {a.Dimension} <> {b.Dimension}");
        }

        T distance = T.Zero;
        for (var i = 0; i < a.Dimension; i++)
            distance = T.Max(distance, T.Abs(a[i] - b[i]));

        return distance;
    }
}
