# Multithreading K-Means++ algorithm

--with multidimensional vectors.

[Vector-3](https://github.com/nikvoronin/k-means-plus-plus/tree/vector-3) branch contains simple but more efficient version of image palette extraction. It is using `System.Numerics.Vector3` type and applied in [The Last Wallpaper](https://github.com/nikvoronin/LastWallpaper) project to make colored mosaic tray icons.

## K-Means Processor

- ProcessorOptions
    - ThreadCount
    - Number of iterations
- VectorN\<T\>
- KmCluster
    - Centroid
- IKmInitializer
    - Simple
    - K++
- IDistanceEstimator
    - Euclidean, +Root
    - Chebyshev
    - Manhattan

`VectorN<T>` where `T` is an any numeric C# type such as integer or floating point, byte, int, double, decimal, etc.

## Im Palette Extractor

TODO

## References

- [k-means clustering](https://en.wikipedia.org/wiki/K-means_clustering). Wikipedia
- [StefanoT/KMeans](https://github.com/StefanoT/KMeans). Simple implementation of K-Means and K-Means++ algorithms in C#
- [kubamaruszczyk1604/KMeansCSharp](https://github.com/kubamaruszczyk1604/KMeansCSharp). C# implementation of k-means-clustering algorithm.
