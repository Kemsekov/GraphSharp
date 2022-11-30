using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp;
/// <summary>
/// Unmanaged ARGB color structure
/// </summary>
public struct UnmanagedColor
{
    /// <summary>
    /// Corresponding color element
    /// </summary>
    public byte A,R,G,B;
    /// <summary>
    /// Color converter
    /// </summary>
    public static explicit operator Color(UnmanagedColor c) => Color.FromArgb(c.A,c.R,c.G,c.B);
}