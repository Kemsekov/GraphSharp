using System;
using System.Collections.Generic;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.GraphDrawer;

/// <summary>
/// Interface for basic drawing operations.
/// </summary>
public interface IShapeDrawer
{
    /// <summary>
    /// Draws text on given position, with given size and color
    /// </summary>
    void DrawText(string text, Vector position, Color color, double size);
    /// <summary>
    /// Fills ellipse on given position, with given with and height and color
    /// </summary>
    void FillEllipse(Vector position, double width, double height, Color color);
    /// <summary>
    /// Draws a line between two points with given thickness and color
    /// </summary>
    void DrawLine(Vector start, Vector end, Color color, double thickness);
    /// <summary>
    /// Clears whole window with <paramref name="color"/>
    /// </summary>
    void Clear(Color color);
}