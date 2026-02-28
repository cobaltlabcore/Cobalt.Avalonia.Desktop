using System.Globalization;
using Avalonia.Media;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

/// <summary>A drawing object that renders a formatted text string at its canvas-space position.</summary>
public class TextShape : DrawingObject
{
    /// <summary>Gets or sets the string to display.</summary>
    public string? Text { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets the font size in points.</summary>
    public double FontSize { get; set => SetProperty(ref field, value); } = 14;

    /// <summary>Gets or sets the font family.</summary>
    public FontFamily FontFamily { get; set => SetProperty(ref field, value); } = FontFamily.Default;

    /// <summary>Gets or sets the font weight.</summary>
    public FontWeight FontWeight { get; set => SetProperty(ref field, value); } = FontWeight.Normal;

    /// <summary>Gets or sets the brush used to paint the text, defaulting to black when <see langword="null"/>.</summary>
    public IBrush? Foreground { get; set => SetProperty(ref field, value); }

    /// <summary>Renders the text at the canvas position defined by <see cref="DrawingObject.CanvasX"/> and <see cref="DrawingObject.CanvasY"/>.</summary>
    /// <param name="context">The drawing context to render into.</param>
    public override void Render(DrawingContext context)
    {
        if (string.IsNullOrEmpty(Text)) return;
        var typeface = new Typeface(FontFamily, FontStyle.Normal, FontWeight);
        var ft = new FormattedText(
            Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            Foreground ?? Brushes.Black);
        context.DrawText(ft, new global::Avalonia.Point(CanvasX, CanvasY));
    }
}
