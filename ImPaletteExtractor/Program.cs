using Colourful;
using KMeans;
using KMeans.DistanceEstimators;
using KMeans.Initializers;
using KMeans.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

const double ResizedImageSize = 400f;
const int ClustersNumber = 10;

var samplePictures =
    Directory.GetFiles(
        Environment.GetFolderPath(
            Environment.SpecialFolder.MyPictures ),
        "*.jpeg" );

IColorConverter<RGBColor, LabColor> rgb2lab = new ConverterBuilder()
    .FromRGB( RGBWorkingSpaces.sRGB )
    .ToLab( Illuminants.D65 )
    .Build();

IColorConverter<LabColor, RGBColor> lab2rgb = new ConverterBuilder()
    .FromLab( Illuminants.D65 )
    .ToRGB( RGBWorkingSpaces.sRGB )
    .Build();

foreach (var picture in samplePictures) {
    using var bitmap = LoadResized( picture );
    VectorN<double>[] points =
        new VectorN<double>[bitmap.Width * bitmap.Height];

    for (var y = 0; y < bitmap.Height; y++) {
        for (var x = 0; x < bitmap.Width; x++) {
            var pixel = bitmap.GetPixel( x, y );
            var lab = rgb2lab.Convert( RGBColor.FromColor( pixel ) );
            points[y * bitmap.Width + x] =
                new VectorN<double>( lab.L, lab.a, lab.b );
        }
    }

    var sw = Stopwatch.StartNew();
    var clusters =
        new KMeansProcessor<double>(
            new KmInitializer<double>(),
            new EuclideanDistance<double>() )
        .Compute( points, ClustersNumber );

    Console.WriteLine( $"Found in {sw.ElapsedMilliseconds / 1000f:.000}s" );

    DrawSamples( picture, clusters );
}

Bitmap LoadResized( string imagePath )
{
    using var src = new Bitmap( imagePath );
    var k =
        src.Width < ResizedImageSize ? 1.0
        : ResizedImageSize / src.Width;
    var dst = new Bitmap( (int)(src.Width * k), (int)(src.Height * k) );

    using var g = Graphics.FromImage( dst );
    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
    g.DrawImage( src, 0, 0, dst.Width, dst.Height );

    return dst;
}

void DrawSamples(
    string imagePath,
    KmCluster<double>[] clusters )
{
    var dstFilename = $"{imagePath}-{Environment.TickCount}.jpg";

    using var dst = Bitmap.FromFile( imagePath );
    using var g = Graphics.FromImage( dst );

    const float margin = 10f;
    var boxSize = (dst.Height - margin) / ClustersNumber - margin;

    for (var i = 0; i < clusters.Length; i++) {
        var labMean = clusters[i].Centroid;
        var labColor = new LabColor( labMean[0], labMean[1], labMean[2] );

        g.FillRectangle(
            new SolidBrush( lab2rgb.Convert( labColor ) ),
            dst.Width - boxSize - margin,
            i * boxSize + margin * (i + 1),
            boxSize, boxSize );
    }

    dst.Save( dstFilename );
}
