using Avalonia;
using Avalonia.Input;
using Avalonia.Media;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

/// <summary>Provides data for the <see cref="Shape.Moved"/> event.</summary>
public sealed class MovedEventArgs : EventArgs
{
    /// <summary>Gets the horizontal distance the shape was moved in world units.</summary>
    public double DeltaX { get; init; }

    /// <summary>Gets the vertical distance the shape was moved in world units.</summary>
    public double DeltaY { get; init; }

    /// <summary>Gets the new world-space X coordinate after the move.</summary>
    public double NewX { get; init; }

    /// <summary>Gets the new world-space Y coordinate after the move.</summary>
    public double NewY { get; init; }
}

/// <summary>
/// Abstract base class for rendered shapes that support fill, stroke, hover styling,
/// hit-testing, and optional drag movement via a <see cref="DragInteraction"/>.
/// </summary>
public abstract class Shape : DrawingObject
{
    /// <summary>Gets or sets a value indicating whether this shape can be dragged by a <see cref="DragInteraction"/>.</summary>
    public bool IsMovable { get; set; }

    /// <summary>Raised after the shape is moved by <see cref="Move"/>.</summary>
    public event EventHandler<MovedEventArgs>? Moved;

    /// <summary>Translates the shape by the given deltas in world units and raises <see cref="Moved"/>.</summary>
    /// <param name="deltaX">Horizontal displacement in world units.</param>
    /// <param name="deltaY">Vertical displacement in world units.</param>
    public void Move(double deltaX, double deltaY)
    {
        X += deltaX;
        Y += deltaY;
        Moved?.Invoke(this, new MovedEventArgs { DeltaX = deltaX, DeltaY = deltaY, NewX = X, NewY = Y });
    }

    /// <summary>Gets or sets the brush used to fill the shape interior.</summary>
    public IBrush? Fill { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets the brush used to draw the shape outline.</summary>
    public IBrush? Stroke { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets the thickness of the stroke in screen pixels.</summary>
    public double StrokeThickness { get; set => SetProperty(ref field, value); } = 1.0;

    /// <summary>Gets or sets the brush used to fill the shape when hovered, overriding <see cref="Fill"/>.</summary>
    public IBrush? FillHover { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets the brush used to draw the outline when hovered, overriding <see cref="Stroke"/>.</summary>
    public IBrush? StrokeHover { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets a dash pattern applied to the stroke, or <see langword="null"/> for a solid line.</summary>
    public IReadOnlyList<double>? StrokeDashArray { get; set; }

    /// <summary>Gets or sets the cursor displayed when the pointer hovers over this shape.</summary>
    public Cursor? Cursor { get; set; }

    internal bool IsHovered { get; set; }

    protected IBrush? EffectiveFill => IsHovered && FillHover is not null ? FillHover : Fill;
    protected IBrush? EffectiveStroke => IsHovered && StrokeHover is not null ? StrokeHover : Stroke;

    protected IPen? BuildPen()
    {
        if (EffectiveStroke is null) return null;
        var dash = StrokeDashArray is not null ? new DashStyle(StrokeDashArray, 0) : null;
        return new Pen(EffectiveStroke, StrokeThickness, dashStyle: dash);
    }

    /// <summary>Returns true if the given canvas-space point is over this shape.</summary>
    /// <remarks>
    /// Inverse-rotates the point into local (unrotated) space, then performs an AABB check.
    /// Subclasses with non-rectangular geometry (ellipse, line…) should override.
    /// </remarks>
    public virtual bool HitTest(Point canvasPoint)
    {
        var dx = canvasPoint.X - (CanvasX + CanvasWidth  / 2);
        var dy = canvasPoint.Y - (CanvasY + CanvasHeight / 2);

        if (Rotation != 0.0)
        {
            var rad = -Rotation * Math.PI / 180.0;
            var cos = Math.Cos(rad);
            var sin = Math.Sin(rad);
            (dx, dy) = (cos * dx - sin * dy, sin * dx + cos * dy);
        }

        return Math.Abs(dx) <= CanvasWidth  / 2 &&
               Math.Abs(dy) <= CanvasHeight / 2;
    }
}
