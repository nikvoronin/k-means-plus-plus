using Colourful;
using KMeans;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;

const float ResizedImageSize = 400f;
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
    Vector3[] points = new Vector3[bitmap.Width * bitmap.Height];

    for (int y = 0; y < bitmap.Height; y++) {
        for (int x = 0; x < bitmap.Width; x++) {
            var pixel = bitmap.GetPixel( x, y );
            var lab = rgb2lab.Convert( RGBColor.FromColor( pixel ) );
            points[y * bitmap.Width + x] =
                new Vector3( (float)lab.L, (float)lab.a, (float)lab.b );
        }
    }

    var sw = Stopwatch.StartNew();
    var clusters =
        new KMeansProcessor( new KppInitializer() )
        .Compute( points, ClustersNumber );

    Console.WriteLine( $"Found in {sw.ElapsedMilliseconds / 1000f:.00}s" );

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
    KmCluster[] clusters )
{
    var dstFilename = $"{imagePath}-{Environment.TickCount}.jpg";

    using var dst = Bitmap.FromFile( imagePath );
    using var g = Graphics.FromImage( dst );

    const float margin = 10f;
    var boxSize = (dst.Height - margin) / ClustersNumber - margin;

    for (int i = 0; i < clusters.Length; i++) {
        var labMean = clusters[i].Centroid;
        var labColor = new LabColor( labMean.X, labMean.Y, labMean.Z );

        g.FillRectangle(
            new SolidBrush( lab2rgb.Convert( labColor ) ),
            dst.Width - boxSize - margin,
            i * boxSize + margin * (i + 1),
            boxSize, boxSize );
    }

    dst.Save( dstFilename );
}
