
// using System.Drawing;
using GraphSharp.Nodes;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.Fonts;
using System.Reflection;

public class GraphDrawer
{
    public IBrush DrawNodeBrush;
    public IBrush DrawLineBrush;
    public float Thickness;
    public float NodeSize;
    public Image<Rgba32> Image;

    public Font Font;

    public GraphDrawer(Image<Rgba32> image, IBrush drawNodeBrush, IBrush drawLineBrush, float fontSize)
    {
        Image = image;
        DrawNodeBrush = drawNodeBrush;
        DrawLineBrush = drawLineBrush;
        FontCollection fonts = new FontCollection();

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "SampleBase.NotoSans-Bold.ttf";
        using (Stream stream = assembly.GetManifestResourceStream(resourceName) ?? Stream.Null)
        {
            fonts.Install(stream);
        }

        var notoSans = fonts.CreateFont("Noto Sans", fontSize * image.Height , FontStyle.Regular);

        Font = notoSans;
    }
    public Image<Rgba32> Clear(Color color)
    {
        Image.Mutate(x => x.Clear(new GraphicsOptions(), new SolidBrush(color)));
        return Image;
    }
    public Image<Rgba32> DrawNodes(IList<INode> nodes)
    {
        Image.Mutate(x =>
        {
            var size = x.GetCurrentSize();

            Parallel.ForEach(nodes, node =>
                DrawNode(x, node, size, NodeSize));
            Parallel.ForEach(nodes, node =>
                DrawNodeId(x, node, size));
        });
        return Image;
    }
    public Image<Rgba32> DrawNodeConnections(IList<INode> nodes)
    {
        Image.Mutate(x =>
        {
            var size = x.GetCurrentSize();
            Parallel.ForEach(nodes, node =>
            {
                foreach (var c in node.Edges)
                {
                    DrawConnection(x, node, c.Node, size);
                }
            });

        });
        return Image;
    }

    public Image<Rgba32> DrawPath(IList<INode> path)
    {
        Dictionary<(int,int),bool> drawn = new();
        Image.Mutate(x =>
        {
            path.Aggregate((n1, n2) =>
            {
                if(drawn.TryGetValue((n1.Id,n2.Id),out var _)) return n2;
                DrawConnection(x, n1, n2, x.GetCurrentSize());
                drawn[(n1.Id,n2.Id)] = true;
                return n2;
            });
        });
        return Image;
    }
    public void DrawNodeId(IImageProcessingContext x, INode node, Size ImageSize)
    {
        if (node is NodeXY nodeXY)
        {

            var point = new PointF((float)nodeXY.X * ImageSize.Width, (float)nodeXY.Y * ImageSize.Height);
            x.DrawText(node.Id.ToString(), Font, Color.Violet, point);
        }
    }
    public void DrawNode(IImageProcessingContext x, INode node, Size ImageSize, float nodeSize)
    {
        if (node is NodeXY nodeXY)
        {
            var point = new PointF((float)nodeXY.X * ImageSize.Width, (float)nodeXY.Y * ImageSize.Height);
            var ellipse = new EllipsePolygon(point, nodeSize * Image.Height);
            x.FillPolygon(new DrawingOptions() { }, DrawNodeBrush, ellipse.Points.ToArray());
        }
    }
    public void DrawConnection(IImageProcessingContext x, INode node1, INode node2, Size ImageSize)
    {
        if (node1 is NodeXY n1 && node2 is NodeXY n2)
        {
            var point1 = new PointF((float)n1.X * ImageSize.Width, (float)n1.Y * ImageSize.Height);
            var point2 = new PointF((float)n2.X * ImageSize.Width, (float)n2.Y * ImageSize.Height);

            x.DrawLines(new DrawingOptions() { }, DrawLineBrush, Thickness * ImageSize.Height, point1, point2);
        }
    }
}