using Colourful;
using KMeans;
using KMeans.DistanceEstimators;
using KMeans.Initializers;
using KMeans.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

const double ResizedImageSize = 400f;
const int ClustersNumber = 10;
const int Dimension = 3; // L*a*b*

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var samplePictures =
    Directory.GetFiles(
        args.Length > 0 ? args[0]
        : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        "*.jpeg");

IColorConverter<RGBColor, LabColor> rgb2lab = new ConverterBuilder()
    .FromRGB(RGBWorkingSpaces.sRGB)
    .ToLab(Illuminants.D65)
    .Build();

IColorConverter<LabColor, RGBColor> lab2rgb = new ConverterBuilder()
    .FromLab(Illuminants.D65)
    .ToRGB(RGBWorkingSpaces.sRGB)
    .Build();

ProcessorOptions options = new()
{
    //ThreadCount = 4,
    //MaxIterations = 100,
    //ConvergenceEpsilon = .01
};

var initializer = new KppInitializer<double>();

var processor =
    new KMeansProcessor<double, EuclideanDistance<double>>(
        initializer,
        options: options);

Console.WriteLine(
    $"""
    Vectors base type = {typeof(double).Name}
    Number of threads = {options.ThreadCount}
    Max iterations = {options.MaxIterations}
    Convergence epsilon = {options.ConvergenceEpsilon}
    Clusters initializer = {initializer.GetType().Name}
    Distance estimator = {typeof(EuclideanDistance<double>).Name}

    """);

foreach (var picture in samplePictures)
{
    Console.WriteLine(new FileInfo(picture).Name);

    using var bitmap = LoadResized(picture);

    // Identical pixels become one weighted point: same result, fewer points.
    var (colors, weights) = CollectColors(bitmap);
    Print($"Unique colors: {colors.Length}");

    var points = new double[colors.Length * Dimension];
    for (var i = 0; i < colors.Length; i++)
    {
        var lab = rgb2lab.Convert(RGBColor.FromColor(Color.FromArgb(colors[i])));
        points[i * Dimension] = lab.L;
        points[i * Dimension + 1] = lab.a;
        points[i * Dimension + 2] = lab.b;
    }

    var sw = Stopwatch.StartNew();
    var result = processor.Compute(points, Dimension, ClustersNumber, weights);
    Print(
        $"Found in {sw.ElapsedMilliseconds / 1000f:.000}s, "
        + $"{result.Iterations} iterations");

    for (var clusterIx = 0; clusterIx < result.ClusterCount; clusterIx++)
    {
        Print($"Cluster {clusterIx}: {result.Weights[clusterIx]} points");
    }

    DrawSamples(picture, result);
}

Bitmap LoadResized(string imagePath)
{
    using var src = new Bitmap(imagePath);

    var k = src.Width < ResizedImageSize 
        ? 1.0
        : ResizedImageSize / src.Width;

    var dst = new Bitmap(
        (int)(src.Width * k), 
        (int)(src.Height * k), 
        PixelFormat.Format32bppArgb);

    Print(
        $"Size: {src.Width}x{src.Height}px, " 
        + $"downscaled to {dst.Width}x{dst.Height}px.");

    using var g = Graphics.FromImage(dst);

    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
    g.DrawImage(src, 0, 0, dst.Width, dst.Height);

    return dst;
}

(int[] Colors, int[] Weights) CollectColors(Bitmap bitmap)
{
    var data = bitmap.LockBits(
        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
        ImageLockMode.ReadOnly,
        PixelFormat.Format32bppArgb);

    try
    {
        var counts = new Dictionary<int, int>();
        var row = new int[bitmap.Width];

        for (var y = 0; y < bitmap.Height; y++)
        {
            Marshal.Copy(data.Scan0 + y * data.Stride, row, 0, row.Length);

            foreach (var argb in row)
            {
                // Ignore alpha: the palette is opaque.
                ref var count = ref CollectionsMarshal.GetValueRefOrAddDefault(
                    counts, argb | unchecked((int)0xFF000000), out _);
                count++;
            }
        }

        var colors = new int[counts.Count];
        var weights = new int[counts.Count];
        var i = 0;

        foreach (var (argb, count) in counts)
        {
            colors[i] = argb;
            weights[i] = count;
            i++;
        }

        return (colors, weights);
    }
    finally
    {
        bitmap.UnlockBits(data);
    }
}

Color ToColor(ReadOnlySpan<double> lab) =>
    lab2rgb.Convert(new LabColor(lab[0], lab[1], lab[2]));

string ToHex(Color color) =>
    $"#{color.R:X2}{color.G:X2}{color.B:X2}";

void DrawSamples(
    string imagePath,
    KMeansResult<double> result)
{
    var dstFilename = $"{imagePath}-{Environment.TickCount}.jpg";

    using var dst = Image.FromFile(imagePath);
    using var g = Graphics.FromImage(dst);

    const float margin = 10f;
    var boxSize = (dst.Height - margin) / ClustersNumber - margin;

    List<string> hexs = [];
    for (var i = 0; i < result.ClusterCount; i++)
    {
        var color = ToColor(result.Centroid(i));

        hexs.Add(ToHex(color));

        using var brush = new SolidBrush(color);
        g.FillRectangle(
            brush,
            dst.Width - boxSize - margin,
            i * boxSize + margin * (i + 1),
            boxSize, boxSize);
    }

    Print("Colors: " + string.Join("; ", hexs));

    dst.Save(dstFilename);
}

void Print(string text) =>
    Console.WriteLine($"  {text}");
