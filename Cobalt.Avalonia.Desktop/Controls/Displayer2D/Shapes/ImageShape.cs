using Avalonia;
using Avalonia.Media;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

/// <summary>A drawing object that renders an <see cref="IImage"/> scaled to fit its bounding box.</summary>
public class ImageShape : DrawingObject
{
    /// <summary>Gets or sets the image to draw.</summary>
    public IImage? Source { get; set => SetProperty(ref field, value); }

    /// <summary>Renders <see cref="Source"/> scaled to fill the canvas bounding box.</summary>
    /// <param name="context">The drawing context to render into.</param>
    public override void Render(DrawingContext context)
    {
        if (Source is null) return;
        var srcSize = Source.Size;
        if (srcSize.Width <= 0 || srcSize.Height <= 0) return;
        var scaleX = CanvasWidth / srcSize.Width;
        var scaleY = CanvasHeight / srcSize.Height;
        using var _ = context.PushTransform(
            Matrix.CreateScale(scaleX, scaleY) * Matrix.CreateTranslation(CanvasX, CanvasY));
        context.DrawImage(Source, new Rect(srcSize));
    }
}
