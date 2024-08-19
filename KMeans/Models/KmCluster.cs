using System.Collections.Generic;
using System.Numerics;

namespace KMeans.Models;

/// <summary>
/// Vector cluster for the K-Means algorithm.
/// </summary>
public class KmCluster<T>
    where T : INumber<T>
{
    public VectorN<T> Centroid {
        get {
            if (_numPoints != _points.Count) {
                _numPoints = _points.Count;
                _centroid = _accumulator / T.CreateChecked( _numPoints );
            }

            return _centroid;
        }
    }

    public IReadOnlyList<VectorN<T>> Points => _points;

    /// <summary>
    /// Create cluster with empty volume.
    /// </summary>
    public KmCluster()
    {
        _points = [];
    }

    /// <summary>
    /// Create cluster with a given centroid vector.
    /// </summary>
    /// <param name="centroid">
    /// Centroid vector of the cluster.
    /// </param>
    public KmCluster( VectorN<T> centroid )
    {
        _points = [centroid];
        _accumulator = centroid;
    }

    /// <summary>
    /// Add vector to a cluster.
    /// </summary>
    /// <param name="point">
    /// New vector to append to the cluster.
    /// </param>
    public void AddPoint( VectorN<T> point )
    {
        _accumulator += point;
        _points.Add( point );
    }

    private VectorN<T> _accumulator = VectorN<T>.Empty;
    private VectorN<T> _centroid = VectorN<T>.Empty;
    private int _numPoints;

    private readonly List<VectorN<T>> _points;
}
