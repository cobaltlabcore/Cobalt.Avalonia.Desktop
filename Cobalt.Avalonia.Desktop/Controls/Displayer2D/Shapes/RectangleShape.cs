using Avalonia;
using Avalonia.Media;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

/// <summary>A shape that renders a rectangle, optionally with rotation applied around the bounding-box centre.</summary>
public partial class RectangleShape : Shape
{
    /// <summary>Gets or sets the X coordinate of the bounding-box centre in world space.</summary>
    public double CenterX { get => X + Width / 2;  set => X = value - Width / 2; }

    /// <summary>Gets or sets the Y coordinate of the bounding-box centre in world space.</summary>
    public double CenterY { get => Y + Height / 2; set => Y = value - Height / 2; }

    /// <summary>Renders the rectangle in canvas space, applying rotation if set.</summary>
    /// <param name="context">The drawing context to render into.</param>
    public override void Render(DrawingContext context)
    {
        using var _ = PushRotation(context);
        context.DrawRectangle(EffectiveFill, BuildPen(), new Rect(CanvasX, CanvasY, CanvasWidth, CanvasHeight));
    }
}
