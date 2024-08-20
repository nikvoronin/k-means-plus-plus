# Multithreading K-Means++ algorithm

--with multidimensional vectors.

[Vector-3](https://github.com/nikvoronin/k-means-plus-plus/tree/vector-3) branch contains simple but more efficient version of image palette extraction. It is using `System.Numerics.Vector3` type and applied in [The Last Wallpaper](https://github.com/nikvoronin/LastWallpaper/tree/main/LastWallpaper/Logic/KMeans) project to make colored mosaic tray icons.

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

||||
|:---:|:---:|:---:|
|<img width=250 src="https://github.com/user-attachments/assets/06fb9fbc-26c7-4ab6-a85a-ff1d10631a3f">|<img width=250 src="https://github.com/user-attachments/assets/e29dd6ea-5072-4748-b8be-53e51fe64124">|<img width=250 src="https://github.com/user-attachments/assets/36878586-f5c3-438d-8aba-f39e1ec4c95c">|
|<img width=250 src="https://github.com/user-attachments/assets/a54fa91a-9af4-4b44-ab8b-a9d1efc728ee">|<img width=250 src="https://github.com/user-attachments/assets/89cfb474-f6d4-4fe9-aecc-05e696204e3c">|<img width=250 src="https://github.com/user-attachments/assets/66dd7a8f-c9da-41bb-bf46-6888150d245e">|
|<img width=250 src="https://github.com/user-attachments/assets/99744581-e937-44d6-874b-a71c1f129b9d">|<img width=250 src="https://github.com/user-attachments/assets/5636986d-7d3b-4e0f-a605-4f9d6d02258e">|<img width=250 src="https://github.com/user-attachments/assets/26899da7-af1c-4641-8c5c-36ccc4e6dbb3">|

## References

- [k-means clustering](https://en.wikipedia.org/wiki/K-means_clustering). Wikipedia
- [StefanoT/KMeans](https://github.com/StefanoT/KMeans). Simple implementation of K-Means and K-Means++ algorithms in C#
- [kubamaruszczyk1604/KMeansCSharp](https://github.com/kubamaruszczyk1604/KMeansCSharp). C# implementation of k-means-clustering algorithm.
