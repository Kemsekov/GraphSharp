using MathNet.Numerics.LinearAlgebra.Single;
using GraphSharp.GraphDrawer;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using GraphSharp.Graphs;
using GraphSharp;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public static class ColorExtensions
{
    public static System.Drawing.Color ToSystemDrawingColor(this Color c)
    {
        var converted = c.ToPixel<Argb32>();
        return System.Drawing.Color.FromArgb((int)converted.Argb);
    }
    public static Color ToImageSharpColor(this System.Drawing.Color c)
    {
        return Color.FromRgba(c.R, c.G, c.B, c.A);
    }
}

public class FontHelper
{
    private static readonly string FileName = "NotoSans-Bold.ttf";
    private static readonly string FileUrl = "https://github.com/Kemsekov/GraphSharp.Samples/raw/refs/heads/main/samples/SampleBase/NotoSans-Bold.ttf";

    public static async Task<string> GetNotoSans()
    {
        string tmpFolder = System.IO.Path.GetTempPath();
        string fontPath = System.IO.Path.Combine(FileName);

        if (!File.Exists(fontPath))
        {
            Console.WriteLine($"Downloading font to {fontPath}");
            using var httpClient = new HttpClient();
            var data = await httpClient.GetByteArrayAsync(FileUrl);
            await File.WriteAllBytesAsync(fontPath, data);
            Console.WriteLine($"Downloaded font to {fontPath}");
        }

        return fontPath;
    }
}


public static class ImageSharpTextHelper
{
    /// <summary>
    /// Draws the specified text centered at the given point on an IImageProcessingContext.
    /// </summary>
    public static void DrawCenteredText(
        this IImageProcessingContext ctx,
        string text,
        Font font,
        Color color,
        PointF center)
    {
        // Measure the size of the text
        FontRectangle rect = TextMeasurer.MeasureSize(text, new TextOptions(font));

        // Compute the top-left position so the text is centered
        var topLeft = new PointF(
            center.X - rect.Width * 0.25f,
            center.Y - rect.Height * 0f);

        // Draw the text
        ctx.DrawText(new RichTextOptions(font) { Origin = topLeft }, text, color);
    }
}

public class ImageSharpShapeDrawer : IShapeDrawer
{
    public static Image<Rgba32> CreateImage<TNode, TEdge>(
        IImmutableGraph<TNode, TEdge> graph,
        Action<GraphDrawer<TNode, TEdge>> draw,
        Func<TNode, Vector> getPos,
        int outputResolution = 1000
    )
    where TNode : INode
    where TEdge : IEdge
    {
        var image = new Image<Rgba32>(outputResolution, outputResolution);
        image.Mutate(x =>
        {
            var shapeDrawer = new ImageSharpShapeDrawer(x, image);
            var drawer = new GraphDrawer<TNode, TEdge>(graph, shapeDrawer, outputResolution, getPos);
            draw(drawer);
        });
        return image;
    }
    public IImageProcessingContext Context { get; }
    public Image<Rgba32> Image { get; }
    public FontCollection Fonts { get; }
    public ImageSharpShapeDrawer(IImageProcessingContext context, Image<Rgba32> image)
    {
        Context = context;
        Image = image;
        FontCollection fonts = new FontCollection();
        // https://github.com/Kemsekov/GraphSharp.Samples/raw/refs/heads/main/samples/SampleBase/NotoSans-Bold.ttf
        var resourceName = FontHelper.GetNotoSans().Result;
        using (var stream = File.OpenRead(resourceName))
        {
            fonts.Add(stream);
        }
        this.Fonts = fonts;
    }

    public void Clear(System.Drawing.Color color)
    {
        Context.Clear(new DrawingOptions(), new SolidBrush(color.ToImageSharpColor()));
    }

    public void DrawLine(Vector start, Vector end, System.Drawing.Color color, double thickness)
    {
        var brush = new SolidBrush(color.ToImageSharpColor());
        Context.DrawLine(brush, (float)thickness, new(start[0], start[1]), new(end[0], end[1]));
    }
    public void DrawText(string text, Vector position, System.Drawing.Color color, double fontSize = -1)
    {
        var font = Fonts.Get("Noto Sans").CreateFont((float)fontSize);
        Context.DrawCenteredText(text, font, color.ToImageSharpColor(), new(position[0], position[1]));
    }

    public void FillEllipse(Vector position, double width, double height, System.Drawing.Color color)
    {
        var ellipse = new EllipsePolygon(new(position[0], position[1]), ((float)(width + height)) / 2);
        var brush = new SolidBrush(color.ToImageSharpColor());
        Context.FillPolygon(new DrawingOptions() { }, brush, ellipse.Points.ToArray());
    }
}
