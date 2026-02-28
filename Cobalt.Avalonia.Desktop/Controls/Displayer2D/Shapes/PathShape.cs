using Avalonia;
using Avalonia.Media;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

/// <summary>
/// A shape that renders an arbitrary <see cref="Avalonia.Media.Geometry"/> with rotation and
/// viewport transform applied. Stroke thickness is compensated for zoom so it remains
/// constant in screen pixels.
/// </summary>
public sealed class PathShape : Shape
{
    /// <summary>Gets or sets the geometry to render. Setting this marks canvas coordinates as dirty.</summary>
    public Geometry? Geometry { get; set { SetProperty(ref field, value); MarkCoordinatesDirty(); } }

    /// <summary>The combined scale-and-translate matrix derived from the current zoom and pan, applied before rendering the geometry.</summary>
    private Matrix _viewportMatrix = Matrix.Identity;

    /// <summary>The zoom factor captured during the last coordinate recalculation, used to compensate stroke thickness.</summary>
    private double _zoom = 1.0;

    /// <summary>Recomputes the viewport transform matrix for the current zoom and pan.</summary>
    /// <param name="zoom">The current zoom factor.</param>
    /// <param name="panX">The horizontal pan offset.</param>
    /// <param name="panY">The vertical pan offset.</param>
    protected override void RecalculateExtraCoordinates(double zoom, double panX, double panY)
    {
        _zoom = IsFixed ? 1.0 : zoom;
        _viewportMatrix = IsFixed
            ? Matrix.Identity
            : Matrix.CreateScale(zoom, zoom) * Matrix.CreateTranslation(panX, panY);
    }

    /// <summary>Renders <see cref="Geometry"/> with fill and a zoom-compensated stroke.</summary>
    /// <param name="context">The drawing context to render into.</param>
    public override void Render(DrawingContext context)
    {
        if (Geometry is null) return;
        var rotation = Matrix.CreateRotation(Rotation * Math.PI / 180.0);
        var combined = rotation * Matrix.CreateTranslation(X, Y) * _viewportMatrix;
        using var _ = context.PushTransform(combined);
        // Compensate stroke thickness for zoom so it remains constant in screen pixels
        var pen = EffectiveStroke is null ? null : new Pen(EffectiveStroke, StrokeThickness / _zoom);
        context.DrawGeometry(EffectiveFill, pen, Geometry);
    }
}
