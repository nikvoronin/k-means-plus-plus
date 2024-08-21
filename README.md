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

Example of console output: _(the values are close to images above but not actual)_

```plain
Vectors base type = Double
Number of threads = 8
Max iterations = 1000
Convergence epsilon = 0.1
Clusters initializer = KppInitializer`1
Distance estimator = EuclideanDistance`1

apod20240814.jpeg
  Size: 4855x3237px, downscaled to 400x266px.
  Found in 3.654s
  Cluster 0: 28077 points
  Cluster 1: 17226 points
  Cluster 2: 3737 points
  Cluster 3: 6950 points
  Cluster 4: 19334 points
  Cluster 5: 5607 points
  Cluster 6: 3494 points
  Cluster 7: 5331 points
  Cluster 8: 6046 points
  Cluster 9: 10598 points
  Colors: #322934; #493745; #7A5A4C; #81436C; #140F08; #815866; #B96788; #AD4A91; #BA8B87; #613C55
bing000.jpeg
  Size: 3840x2160px, downscaled to 400x225px.
  Found in 4.091s
  Cluster 0: 14070 points
  Cluster 1: 5400 points
  Cluster 2: 8556 points
  Cluster 3: 5607 points
  Cluster 4: 12713 points
  Cluster 5: 10196 points
  Cluster 6: 13601 points
  Cluster 7: 5211 points
  Cluster 8: 6117 points
  Cluster 9: 8529 points
  Colors: #2F2C4D; #B9532C; #504385; #733B36; #1C1A1E; #9088C8; #594C64; #8D824A; #CFCCEC; #474838
  ...
wikipedia20240814.jpeg
  Size: 8256x5504px, downscaled to 400x266px.
  Found in 2.088s
  Cluster 0: 1816 points
  Cluster 1: 46070 points
  Cluster 2: 7794 points
  Cluster 3: 5663 points
  Cluster 4: 2834 points
  Cluster 5: 8727 points
  Cluster 6: 14064 points
  Cluster 7: 2627 points
  Cluster 8: 15491 points
  Cluster 9: 1314 points
  Colors: #7A7328; #787873; #D7761D; #C1401A; #B0AA40; #CBB9A2; #A3A19B; #682814; #F9B139; #A18675
```

## References

- [k-means clustering](https://en.wikipedia.org/wiki/K-means_clustering). Wikipedia
- [StefanoT/KMeans](https://github.com/StefanoT/KMeans). Simple implementation of K-Means and K-Means++ algorithms in C#
- [kubamaruszczyk1604/KMeansCSharp](https://github.com/kubamaruszczyk1604/KMeansCSharp). C# implementation of k-means-clustering algorithm.
