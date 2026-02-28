using Avalonia;
using Avalonia.Media;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

/// <summary>
/// A shape that renders a straight line from
/// (<see cref="DrawingObject.X"/>, <see cref="DrawingObject.Y"/>) to (<see cref="X2"/>, <see cref="Y2"/>).
/// </summary>
public sealed class LineShape : Shape
{
    /// <summary>Gets or sets the world-space X coordinate of the line's second endpoint.</summary>
    public double X2 { get; set { SetProperty(ref field, value); MarkCoordinatesDirty(); } }

    /// <summary>Gets or sets the world-space Y coordinate of the line's second endpoint.</summary>
    public double Y2 { get; set { SetProperty(ref field, value); MarkCoordinatesDirty(); } }

    /// <summary>Gets the canvas-space X coordinate of the second endpoint.</summary>
    public double CanvasX2 { get; private set; }

    /// <summary>Gets the canvas-space Y coordinate of the second endpoint.</summary>
    public double CanvasY2 { get; private set; }

    /// <summary>Recomputes the canvas-space coordinates for the second endpoint.</summary>
    /// <param name="zoom">The current zoom factor.</param>
    /// <param name="panX">The horizontal pan offset.</param>
    /// <param name="panY">The vertical pan offset.</param>
    protected override void RecalculateExtraCoordinates(double zoom, double panX, double panY)
    {
        CanvasX2 = IsFixed ? X2 : X2 * zoom + panX;
        CanvasY2 = IsFixed ? Y2 : Y2 * zoom + panY;
    }

    /// <summary>Draws the line using the current stroke brush.</summary>
    /// <param name="context">The drawing context to render into.</param>
    public override void Render(DrawingContext context)
    {
        var pen = BuildPen();
        if (pen is null) return;
        context.DrawLine(pen, new Point(CanvasX, CanvasY),
                              new Point(CanvasX2, CanvasY2));
    }

    /// <summary>Returns <see langword="true"/> if <paramref name="p"/> is within 5 canvas pixels of the line segment.</summary>
    /// <param name="p">The canvas-space point to test.</param>
    /// <returns><see langword="true"/> if the point is close enough to the segment to be considered a hit.</returns>
    public override bool HitTest(Point p)
    {
        const double tolerance = 5.0;
        var ax = CanvasX2 - CanvasX;
        var ay = CanvasY2 - CanvasY;
        var lenSq = ax * ax + ay * ay;
        if (lenSq == 0) return Math.Abs(p.X - CanvasX) <= tolerance && Math.Abs(p.Y - CanvasY) <= tolerance;
        var t = Math.Clamp(((p.X - CanvasX) * ax + (p.Y - CanvasY) * ay) / lenSq, 0, 1);
        var dx = p.X - (CanvasX + t * ax);
        var dy = p.Y - (CanvasY + t * ay);
        return dx * dx + dy * dy <= tolerance * tolerance;
    }
}
