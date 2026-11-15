using System;
using System.Diagnostics.CodeAnalysis;

namespace KMeans;

internal static class ThrowHelper
{
    [DoesNotReturn]
    public static void DimensionMismatch(int left, int right) =>
        throw new ArgumentOutOfRangeException(
            "b",
            $"Vectors dimension mismatch: {left} <> {right}");
}
