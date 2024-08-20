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

```plain
Found in 1.209s
    #B88A86; #EA4; #80436C; #694259; #B46885; #4A3747; #AD4B90; #332934; #7D5B54; #201A11
Found in 1.233s
    #8B8149; #9088C9; #1B181C; #723A35; #464738; #4E4285; #302D4B; #CFCDEC; #B8532C; #5D4E67
Found in 1.370s
    #46382C; #9D8E7E; #7C6F62; #1E2116; #CFDBEA; #384323; #879EB2; #605347; #C9B49C; #656E37
Found in 1.442s
    #EBB52; #2D2945; #166197; #5B6767; #46ACA3; #B1D7E0; #62A4CB; #BF91C; #C2C39C; #923A24
Found in 1.199s
    #1A4A13; #727F16; #C36BCA; #829A75; #35651B; #884B75; #B19DAF; #D02FD7; #A31C94; #374B42
Found in .989s
    #CE7C3; #FFE063; #90513; #B1653; #713F3; #D7941C; #ECAD40; #9C676; #BB7BA; #FED45A
Found in .808s
    #DCA9A0; #B4B5B9; #D4C1B9; #C3C3BF; #CCB9BB; #E0BAAF; #B9B3BC; #A5B2B9; #C6938F; #CEACA4
Found in .628s
    #777772; #F8BC4B; #D5741D; #672A15; #CDB396; #989438; #BE3E19; #9A9790; #F7A626; #BCB7AD
```

## References

- [k-means clustering](https://en.wikipedia.org/wiki/K-means_clustering). Wikipedia
- [StefanoT/KMeans](https://github.com/StefanoT/KMeans). Simple implementation of K-Means and K-Means++ algorithms in C#
- [kubamaruszczyk1604/KMeansCSharp](https://github.com/kubamaruszczyk1604/KMeansCSharp). C# implementation of k-means-clustering algorithm.
