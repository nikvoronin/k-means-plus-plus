# Fast K-Means++

Fast K-Means++ clustering for N-dimensional numerical data, with SIMD, parallel processing and Hamerly acceleration.

||||
|:---:|:---:|:---:|
|<img width=250 src="https://github.com/user-attachments/assets/89cfb474-f6d4-4fe9-aecc-05e696204e3c">|<img width=250 src="https://github.com/user-attachments/assets/66dd7a8f-c9da-41bb-bf46-6888150d245e">|<img width=210 src="https://github.com/user-attachments/assets/26899da7-af1c-4641-8c5c-36ccc4e6dbb3">|

- with multidimensional vectors
- Hamerly acceleration
- SIMD distances
- weighted points
- no per-point allocations
- and deterministic results for a fixed seed.

[Vector-3](https://github.com/nikvoronin/k-means-plus-plus/tree/vector-3) branch contains a simpler version of image palette extraction.
It uses `System.Numerics.Vector3` type and applied in [The Last Wallpaper](https://github.com/nikvoronin/LastWallpaper/tree/main/LastWallpaper/Logic/KMeans) project to make colored mosaic tray icons. The [current version](#results-compared-to-legacy-net8) is about 5 times faster than it on the same `float` data and allocates 65 times less memory.

## K-Means Processor

- `KMeansProcessor<T, TDistance>.Compute(points, dimension, clusterCount, weights?)`
    - `points` is a flat row-major buffer: n × `dimension` values
    - optional `weights`: weight of every point, e.g. the number of duplicates
- `KMeansResult<T>`
    - `Centroids` (row-major), `Centroid(i)`
    - `Labels` – cluster index of every point
    - `Weights` – number of points (sum of weights) in every cluster
    - `Iterations`
- `ProcessorOptions`
    - `ThreadCount`
    - `MaxIterations`
    - `ConvergenceEpsilon`
    - `Seed` – deterministic result for the same seed
    - `UseHamerlyBounds` (default `true`) – skip distance computations with the Hamerly bounds;
      same result, applied only to metrics (`IsMetric`), not to `SquaredEuclideanDistance`
- `IKmInitializer<T>`
    - `KmInitializer` – random distinct points
    - `KppInitializer` – K-Means++ (weighted D² seeding)
- `IDistanceEstimator<T>` – stateless `struct`s, so the JIT inlines them into the hot loop;
  `IsMetric` tells whether the triangle inequality holds
    - `EuclideanDistance`, `SquaredEuclideanDistance`
    - `ChebyshevDistance`
    - `ManhattanDistance`

`T` is any unmanaged numeric C# type: `double`, `float`, `int`, `byte`, `decimal`, etc.
Distances use `Vector<T>` SIMD when the dimension is at least `Vector<T>.Count`.

```csharp
var processor = new KMeansProcessor<double, EuclideanDistance<double>>(
    new KppInitializer<double>(),
    options: new() { Seed = 42 });

var result = processor.Compute(points, dimension: 3, clusterCount: 10);
var firstCentroid = result.Centroid(0);
```

### Benchmarks

`KMeans.Benchmarks`: 120 000 points from 10 Gaussian blobs, 10 clusters, 20 iterations at most.
`ComputeLloyd` and `ComputeHamerly` compare the iterations without and with the Hamerly bounds.

```plain
dotnet run -c Release --project KMeans.Benchmarks -- --filter '*'
```

### Results compared to `legacy-net8`

Same workload as above, BenchmarkDotNet, .NET 10.0.15, x64, AVX2, 22 logical cores.
The `legacy-net8` code was run on the same .NET 10 runtime, so the numbers show the code changes only.

| Benchmark | Dimension | `legacy-net8` | Lloyd | Hamerly (default) | Speed-up |
| --- | ---: | ---: | ---: | ---: | ---: |
| Compute: time | 3 | 364 ms | 33.5 ms | **18.4 ms** | ×20 |
| Compute: allocated | 3 | 488 MB | 1.8 MB | 3.6 MB | ×135 less |
| Compute: time | 16 | 263–407 ms¹ | 72.8 ms | **23.8 ms** | ×11–17 |
| Compute: allocated | 16 | 968 MB | 2.5 MB | 4.3 MB | ×224 less |
| K-Means++ initialization | 3 | 65 ms | 4.6 ms | 4.6 ms | ×14 |
| K-Means++ initialization | 16 | 165 ms | 6.3 ms | 6.3 ms | ×26 |

¹ The legacy run was noisy (median 263 ms, mean 407 ms): its initialization was not seeded.

Where it comes from:

- No allocations per point: the legacy version created a new vector on every point addition
  and copied all points between clusters on every iteration.
- Flat row-major points instead of an object per point (24 instead of ~80 bytes for 3 doubles).
- Distance estimators are `struct`s inlined into the hot loop; no square root to find the nearest centroid;
  `Vector<T>` SIMD for wide vectors.
- Hamerly bounds skip most distance computations once the clusters settle.
- K-Means++ updates the distances to the newest centroid only: O(n·k) instead of O(n·k²), in parallel.
- Small fixed ranges with ordered merging: balanced load on hybrid CPUs and deterministic sums.
- The palette extractor reads pixels with `LockBits` instead of `GetPixel`
  and merges identical colors into weighted points (42 000 points per image on average, see below).

### Palette extraction on real images

`ImPaletteExtractor` on 256 wallpaper photos (JPEG, downscaled to 400 px wide), 10 clusters,
time of the clustering only (`Found in`), images processed one by one:

| | Total | Mean | Median | 90th percentile |
| --- | ---: | ---: | ---: | ---: |
| `legacy-net8` on .NET 8 | 160.7 s | 628 ms | 528 ms | 1069 ms |
| `legacy-net8` on .NET 10 | 154.9 s | 605 ms | 534 ms | 922 ms |
| current version | **3.0 s** | **11.8 ms** | **9 ms** | **17 ms** |

About 50 times faster in total and per image. The newer runtime alone gives the legacy code only 4%.
The current version clusters the unique colors with their pixel counts as weights
(42 000 points per image on average instead of up to 240 000 pixels), the legacy one clusters every pixel.
Neither version was seeded, so the initial centroids and the number of iterations vary between runs.

## Im Palette Extractor

||||
|:---:|:---:|:---:|
|<img width=250 src="https://github.com/user-attachments/assets/06fb9fbc-26c7-4ab6-a85a-ff1d10631a3f">|<img width=250 src="https://github.com/user-attachments/assets/e29dd6ea-5072-4748-b8be-53e51fe64124">|<img width=250 src="https://github.com/user-attachments/assets/36878586-f5c3-438d-8aba-f39e1ec4c95c">|
|<img width=250 src="https://github.com/user-attachments/assets/a54fa91a-9af4-4b44-ab8b-a9d1efc728ee">|<img width=250 src="https://github.com/user-attachments/assets/89cfb474-f6d4-4fe9-aecc-05e696204e3c">|<img width=250 src="https://github.com/user-attachments/assets/66dd7a8f-c9da-41bb-bf46-6888150d245e">|
|<img width=250 src="https://github.com/user-attachments/assets/99744581-e937-44d6-874b-a71c1f129b9d">|<img width=250 src="https://github.com/user-attachments/assets/5636986d-7d3b-4e0f-a605-4f9d6d02258e">|<img width=250 src="https://github.com/user-attachments/assets/26899da7-af1c-4641-8c5c-36ccc4e6dbb3">|

Usage: `ImPaletteExtractor [folder]` processes `*.jpeg` from the folder (default: the Pictures folder).
Identical pixels are merged into one weighted point before clustering.

Example of console output: _(the values are close to images above but not actual; the current version also prints the number of unique colors and iterations)_

```text
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
