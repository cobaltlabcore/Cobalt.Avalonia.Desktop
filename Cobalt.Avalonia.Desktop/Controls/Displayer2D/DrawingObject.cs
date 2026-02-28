using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia;
using Avalonia.Media;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D;

/// <summary>
/// Abstract base class for all objects rendered on a <see cref="Displayer2D"/> canvas.
/// Manages world-space position, size, rotation, and visibility, and converts them to
/// canvas-space coordinates on demand using the current zoom factor and pan offsets.
/// </summary>
public abstract class DrawingObject : ObservableObject
{
    /// <summary>Gets or sets the world-space X coordinate of the top-left corner.</summary>
    public double X { get; set { SetProperty(ref field, value); MarkCoordinatesDirty(); } }

    /// <summary>Gets or sets the world-space Y coordinate of the top-left corner.</summary>
    public double Y { get; set { SetProperty(ref field, value); MarkCoordinatesDirty(); } }

    /// <summary>Gets or sets the draw order; higher values are rendered on top of lower values.</summary>
    public int ZIndex { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets the world-space width of the object's bounding box.</summary>
    public double Width { get; set { SetProperty(ref field, value); MarkCoordinatesDirty(); } } = 100;

    /// <summary>Gets or sets the world-space height of the object's bounding box.</summary>
    public double Height { get; set { SetProperty(ref field, value); MarkCoordinatesDirty(); } } = 100;

    /// <summary>Gets or sets the clockwise rotation angle in degrees around the bounding-box centre.</summary>
    public double Rotation { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets a value indicating whether this object is rendered.</summary>
    public bool IsVisible { get; set => SetProperty(ref field, value); } = true;

    /// <summary>Gets or sets a value indicating whether this object is fixed to screen space, ignoring zoom and pan.</summary>
    public bool IsFixed { get; set { SetProperty(ref field, value); MarkCoordinatesDirty(); } }

    /// <summary>Gets or sets a value indicating whether the object's width is fixed in screen pixels while its position still follows zoom and pan.</summary>
    public bool IsFixedWidth { get; set { SetProperty(ref field, value); MarkCoordinatesDirty(); } }

    /// <summary>Gets or sets a value indicating whether the object's height is fixed in screen pixels while its position still follows zoom and pan.</summary>
    public bool IsFixedHeight { get; set { SetProperty(ref field, value); MarkCoordinatesDirty(); } }

    /// <summary>Gets the computed X coordinate in canvas (screen) space.</summary>
    public double CanvasX { get; protected set; }

    /// <summary>Gets the computed Y coordinate in canvas (screen) space.</summary>
    public double CanvasY { get; protected set; }

    /// <summary>Gets the computed width in canvas (screen) space.</summary>
    public double CanvasWidth { get; protected set; }

    /// <summary>Gets the computed height in canvas (screen) space.</summary>
    public double CanvasHeight { get; protected set; }

    private bool _coordinatesDirty = true;
    private double _lastZoom, _lastPanX, _lastPanY;

    /// <summary>Marks the canvas-space coordinates as stale so they are recomputed on the next <see cref="RecalculateCoordinates"/> call.</summary>
    protected void MarkCoordinatesDirty() => _coordinatesDirty = true;

    /// <summary>
    /// Recomputes canvas-space coordinates from the current zoom and pan offsets.
    /// The update is skipped when the inputs have not changed and no property has been modified.
    /// </summary>
    /// <param name="zoom">The current zoom multiplier.</param>
    /// <param name="panX">The horizontal pan offset in canvas pixels.</param>
    /// <param name="panY">The vertical pan offset in canvas pixels.</param>
    public void RecalculateCoordinates(double zoom, double panX, double panY)
    {
        if (!_coordinatesDirty && _lastZoom == zoom && _lastPanX == panX && _lastPanY == panY)
            return;

        if (IsFixed)
        {
            CanvasX = X; CanvasY = Y; CanvasWidth = Width; CanvasHeight = Height;
        }
        else
        {
            if (IsFixedWidth)
            {
                CanvasWidth = Width;
                CanvasX = (X + Width / 2) * zoom + panX - Width / 2;
            }
            else
            {
                CanvasX = X * zoom + panX;
                CanvasWidth = Width * zoom;
            }

            if (IsFixedHeight)
            {
                CanvasHeight = Height;
                CanvasY = (Y + Height / 2) * zoom + panY - Height / 2;
            }
            else
            {
                CanvasY = Y * zoom + panY;
                CanvasHeight = Height * zoom;
            }
        }

        _coordinatesDirty = false;
        _lastZoom = zoom; _lastPanX = panX; _lastPanY = panY;

        RecalculateExtraCoordinates(zoom, panX, panY);
    }

    /// <summary>
    /// Override to recompute any additional canvas-space coordinates (such as a line's second endpoint)
    /// after the base bounding box has been updated.
    /// </summary>
    /// <param name="zoom">The current zoom multiplier.</param>
    /// <param name="panX">The horizontal pan offset in canvas pixels.</param>
    /// <param name="panY">The vertical pan offset in canvas pixels.</param>
    protected virtual void RecalculateExtraCoordinates(double zoom, double panX, double panY) { }

    /// <summary>
    /// Pushes a rotation transform around the bounding-box centre.
    /// Returns null (and does nothing) when <see cref="Rotation"/> is zero.
    /// </summary>
    protected IDisposable? PushRotation(DrawingContext context)
    {
        if (Rotation == 0.0) return null;
        var cx = CanvasX + CanvasWidth / 2;
        var cy = CanvasY + CanvasHeight / 2;
        var rad = Rotation * Math.PI / 180.0;
        var m = Matrix.CreateTranslation(-cx, -cy)
                * Matrix.CreateRotation(rad)
                * Matrix.CreateTranslation(cx, cy);
        return context.PushTransform(m);
    }

    /// <summary>Renders this object onto the provided drawing context.</summary>
    /// <param name="context">The drawing context to render into.</param>
    public abstract void Render(DrawingContext context);
}
