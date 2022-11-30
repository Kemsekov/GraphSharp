using System.Drawing;
namespace GraphSharp.Common;

///<inheritdoc/>
public interface IColored
{
    /// <summary>
    /// Color assigned to given element
    /// </summary>
    Color Color { get; set; }
}