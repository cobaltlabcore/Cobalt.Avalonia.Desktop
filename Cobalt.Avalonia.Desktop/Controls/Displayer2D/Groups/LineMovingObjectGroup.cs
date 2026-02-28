using Avalonia.Media;
using Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D.Groups;

/// <summary>
/// A <see cref="DrawingObjectGroup"/> that manages a movable line defined by two endpoint handles.
/// The line body acts as a drag target to translate both endpoints simultaneously; each endpoint
/// handle can also be dragged independently to reposition it.
/// </summary>
public sealed class LineMovingObjectGroup : DrawingObjectGroup
{
    private const double HitboxThickness = 20.0;

    private readonly LineShape _line;
    private readonly CircleShape _point1;
    private readonly CircleShape _point2;
    private readonly RectangleShape _hitbox;
    private readonly CircleShape _point1Hitbox;
    private readonly CircleShape _point2Hitbox;

    /// <summary>Gets or sets the stroke brush of the line.</summary>
    public IBrush? LineStroke
    {
        get => _line.Stroke;
        set => _line.Stroke = value;
    }

    /// <summary>Gets or sets the stroke thickness of the line in screen pixels.</summary>
    public double LineStrokeThickness
    {
        get => _line.StrokeThickness;
        set => _line.StrokeThickness = value;
    }

    /// <summary>Initializes a new <see cref="LineMovingObjectGroup"/> with the given endpoint world coordinates.</summary>
    /// <param name="x1">World-space X coordinate of the first endpoint.</param>
    /// <param name="y1">World-space Y coordinate of the first endpoint.</param>
    /// <param name="x2">World-space X coordinate of the second endpoint.</param>
    /// <param name="y2">World-space Y coordinate of the second endpoint.</param>
    public LineMovingObjectGroup(double x1, double y1, double x2, double y2)
    {
        _line = new LineShape
        {
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 2.0,
            ZIndex = 0
        };

        _point1 = new CircleShape
        {
            IsFixedWidth = true,
            IsFixedHeight = true,
            Width = 12,
            Height = 12,
            Fill = new SolidColorBrush(Color.Parse("#3574F0")),
            FillHover = new SolidColorBrush(Colors.White),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 1.5,
            ZIndex = 2
        };

        _point2 = new CircleShape
        {
            IsFixedWidth = true,
            IsFixedHeight = true,
            Width = 12,
            Height = 12,
            Fill = new SolidColorBrush(Color.Parse("#3574F0")),
            FillHover = new SolidColorBrush(Colors.White),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 1.5,
            ZIndex = 2
        };

        _hitbox = new RectangleShape
        {
            IsMovable = true,
            IsFixedHeight = true,
            Stroke = null,
            ZIndex = 3
        };

        _point1Hitbox = new CircleShape
        {
            IsFixedWidth = true,
            IsFixedHeight = true,
            Width = 24,
            Height = 24,
            IsMovable = true,
            ZIndex = 4
        };

        _point2Hitbox = new CircleShape
        {
            IsFixedWidth = true,
            IsFixedHeight = true,
            Width = 24,
            Height = 24,
            IsMovable = true,
            ZIndex = 4
        };

        _point1.CenterX = x1;
        _point1.CenterY = y1;
        _point2.CenterX = x2;
        _point2.CenterY = y2;
        _point1Hitbox.CenterX = x1;
        _point1Hitbox.CenterY = y1;
        _point2Hitbox.CenterX = x2;
        _point2Hitbox.CenterY = y2;

        Items.Add(_line);
        Items.Add(_hitbox);
        Items.Add(_point1);
        Items.Add(_point2);
        Items.Add(_point1Hitbox);
        Items.Add(_point2Hitbox);

        _point1Hitbox.Moved += OnPointMoved;
        _point2Hitbox.Moved += OnPointMoved;
        _hitbox.Moved += OnHitboxMoved;
    }

    /// <inheritdoc/>
    public override void RecalculateCoordinates()
    {
        var x1 = _point1Hitbox.CenterX;
        var y1 = _point1Hitbox.CenterY;
        var x2 = _point2Hitbox.CenterX;
        var y2 = _point2Hitbox.CenterY;
        
        _point1.CenterX = x1;
        _point1.CenterY = y1;
        _point2.CenterX = x2;
        _point2.CenterY = y2;

        _line.X  = x1; _line.Y  = y1;
        _line.X2 = x2; _line.Y2 = y2;

        var dx = x2 - x1;
        var dy = y2 - y1;
        var length = Math.Sqrt(dx * dx + dy * dy);
        var angle  = Math.Atan2(dy, dx) * 180.0 / Math.PI;

        _hitbox.Width   = length;
        _hitbox.Height  = HitboxThickness;
        _hitbox.CenterX = (x1 + x2) / 2.0;
        _hitbox.CenterY = (y1 + y2) / 2.0;
        _hitbox.Rotation = angle;
    }

    private void OnPointMoved(object? sender, MovedEventArgs e)
    {
        RecalculateCoordinates();
    }

    private void OnHitboxMoved(object? sender, MovedEventArgs e)
    {
        _point1Hitbox.CenterX += e.DeltaX;
        _point1Hitbox.CenterY += e.DeltaY;
        _point2Hitbox.CenterX += e.DeltaX;
        _point2Hitbox.CenterY += e.DeltaY;
        RecalculateCoordinates();
    }

    /// <inheritdoc/>
    public override void UnregisterEvents()
    {
        _point1.Moved -= OnPointMoved;
        _point2.Moved -= OnPointMoved;
        _hitbox.Moved -= OnHitboxMoved;
        UnregisterCollectionEvents();
    }
}
