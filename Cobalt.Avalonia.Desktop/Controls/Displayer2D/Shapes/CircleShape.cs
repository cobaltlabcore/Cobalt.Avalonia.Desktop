using Avalonia;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

/// <summary>
/// An <see cref="EllipseShape"/> with equal width and height (default 6×6) that uses an
/// exact radial distance hit-test, avoiding the need for trigonometry.
/// </summary>
public class CircleShape : EllipseShape
{
    /// <summary>Initializes a new <see cref="CircleShape"/> with a default diameter of 6 world units.</summary>
    public CircleShape()
    {
        Width  = 6.0;
        Height = 6.0;
    }

    /// <summary>Returns <see langword="true"/> if <paramref name="canvasPoint"/> falls within the circle's radius.</summary>
    /// <param name="canvasPoint">The canvas-space point to test.</param>
    /// <returns><see langword="true"/> if the point is inside the circle.</returns>
    public override bool HitTest(Point canvasPoint)
    {
        var dx = canvasPoint.X - (CanvasX + CanvasWidth  / 2);
        var dy = canvasPoint.Y - (CanvasY + CanvasHeight / 2);
        var r  = CanvasWidth / 2;
        return dx * dx + dy * dy <= r * r;
    }
}
