using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;



namespace GraphSharp.GraphDrawer
{
    /// <summary>
    /// Interface for base drawing operations.
    /// </summary>
    public interface IShapeDrawer
    {
        /// <summary>
        /// Window size
        /// </summary>
        PointF Size { get; }
        void DrawText(string text, Vector2 position, Color color);
        void FillEllipse(Vector2 position, float width, float height, Color color);
        void DrawLine(Vector2 start, Vector2 end, Color color, float thickness);
        void Clear(Color color);
    }
}