using Avalonia;
using Avalonia.Media;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

/// <summary>
/// A shape that renders an ellipse, with hit-testing using the ellipse equation
/// in inverse-rotated local space.
/// </summary>
public class EllipseShape : Shape
{
    /// <summary>Gets or sets the X coordinate of the ellipse centre in world space.</summary>
    public double CenterX
    {
        get => X + Width / 2;
        set => X = value - Width / 2;
    }

    /// <summary>Gets or sets the Y coordinate of the ellipse centre in world space.</summary>
    public double CenterY
    {
        get => Y + Height / 2;
        set => Y = value - Height / 2;
    }

    /// <summary>Renders the ellipse in canvas space, applying rotation if set.</summary>
    /// <param name="context">The drawing context to render into.</param>
    public override void Render(DrawingContext context)
    {
        using var _ = PushRotation(context);
        context.DrawEllipse(EffectiveFill, BuildPen(),
            new global::Avalonia.Point(CanvasX + CanvasWidth / 2, CanvasY + CanvasHeight / 2),
            CanvasWidth / 2, CanvasHeight / 2);
    }

    /// <inheritdoc/>
    /// Uses the ellipse equation (dx²/a² + dy²/b² ≤ 1) in inverse-rotated local space.
    public override bool HitTest(Point canvasPoint)
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

        var a = CanvasWidth  / 2;
        var b = CanvasHeight / 2;
        if (a <= 0 || b <= 0) return false;
        return (dx * dx) / (a * a) + (dy * dy) / (b * b) <= 1.0;
    }
}
