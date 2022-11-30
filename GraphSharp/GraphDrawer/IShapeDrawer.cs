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
    /// <summary>
    /// Draws text on given position, with given size and color
    /// </summary>
    void DrawText(string text, Vector2 position, Color color, double size);
    /// <summary>
    /// Fills ellipse on given position, with given with and height and color
    /// </summary>
    void FillEllipse(Vector2 position, double width, double height, Color color);
    /// <summary>
    /// Draws a line between two points with given thickness and color
    /// </summary>
    void DrawLine(Vector2 start, Vector2 end, Color color, double thickness);
    /// <summary>
    /// Clears whole window with <paramref name="color"/>
    /// </summary>
    void Clear(Color color);
}