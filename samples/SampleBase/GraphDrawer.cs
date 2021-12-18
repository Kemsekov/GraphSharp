
// using System.Drawing;
using GraphSharp.Nodes;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.Fonts;
public class GraphDrawer
{
    public IBrush DrawNodeBrush;
    public float Radius;
    public IBrush DrawLineBrush;
    public float Thickness;


    public Font Font;

    public GraphDrawer(IBrush drawNodeBrush, float radius, IBrush drawLineBrush, float thickness, float fontSize)
    {
        DrawNodeBrush = drawNodeBrush;
        Radius = radius;
        DrawLineBrush = drawLineBrush;
        Thickness = thickness;

        Font = SystemFonts.CreateFont("Noto Sans", fontSize, FontStyle.Regular);
    }
    public Image<Rgba32> Clear(Image<Rgba32> image, Color color)
    {
        image.Mutate(x => x.Clear(new GraphicsOptions(), new SolidBrush(color)));
        return image;
    }
    public Image<Rgba32> DrawNodes(Image<Rgba32> image, IList<INode> nodes)
    {
        image.Mutate(x =>
        {
            var size = x.GetCurrentSize();

            Parallel.ForEach(nodes, node =>
                DrawNode(x, node, size));
            Parallel.ForEach(nodes, node =>
                DrawNodeId(x,node,size));
        });
        return image;
    }
    public Image<Rgba32> DrawNodeConnections(Image<Rgba32> image, IList<INode> nodes)
    {
        image.Mutate(x =>
        {
            var size = x.GetCurrentSize();
            Parallel.ForEach(nodes, node =>
            {
                foreach (var c in node.Children)
                {
                    DrawConnection(x, node, c.Node, size);
                }
            });
            
        });
        return image;
    }

    public Image<Rgba32> DrawPath(Image<Rgba32> image, IList<INode> path)
    {
        image.Mutate(x =>
        {

            path.Aggregate((n1, n2) =>
            {
                DrawConnection(x, n1, n2, x.GetCurrentSize());
                return n2;
            });
        });
        return image;
    }
    public void DrawNodeId(IImageProcessingContext x, INode node, Size imageSize)
    {
        if (node is NodeXY nodeXY)
        {

            var point = new PointF((float)nodeXY.X * imageSize.Width, (float)nodeXY.Y * imageSize.Height);
            x.DrawText(node.Id.ToString(), Font, Color.Violet, point);
        }
    }
    public void DrawNode(IImageProcessingContext x, INode node, Size imageSize)
    {
        if (node is NodeXY nodeXY)
        {
            var point = new PointF((float)nodeXY.X * imageSize.Width, (float)nodeXY.Y * imageSize.Height);
            var ellipse = new EllipsePolygon(point, 0.01f * (imageSize.Width + imageSize.Height) / 2);
            x.FillPolygon(new DrawingOptions() { }, DrawNodeBrush, ellipse.Points.ToArray());
        }
    }
    public void DrawConnection(IImageProcessingContext x, INode node1, INode node2, Size imageSize)
    {
        if (node1 is NodeXY n1 && node2 is NodeXY n2)
        {
            var point1 = new PointF((float)n1.X * imageSize.Width, (float)n1.Y * imageSize.Height);
            var point2 = new PointF((float)n2.X * imageSize.Width, (float)n2.Y * imageSize.Height);

            x.DrawLines(new DrawingOptions() { }, DrawLineBrush, Thickness * (imageSize.Height + imageSize.Width) / 2, point1, point2);
        }
    }
}