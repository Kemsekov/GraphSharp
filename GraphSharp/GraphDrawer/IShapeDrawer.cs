using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
namespace GraphSharp.GraphDrawer;

/// <summary>
/// Interface for basic drawing operations.
/// </summary>
public interface IShapeDrawer
{
    void DrawText(string text, Vector2 position, Color color, double size);
    void FillEllipse(Vector2 position, double width, double height, Color color);
    void DrawLine(Vector2 start, Vector2 end, Color color, double thickness);
    /// <summary>
    /// Clears whole window with <paramref name="color"/>
    /// </summary>
    void Clear(Color color);
}