using Avalonia.Input;
using Avalonia.Media;
using Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D.Groups;

/// <summary>
/// A <see cref="DrawingObjectGroup"/> that represents a rectangular region of interest (ROI)
/// defined by two spine endpoints (A and B) and a half-width measured perpendicularly to the spine.
/// Provides interactive handles for moving the body, repositioning each endpoint, and adjusting the width.
/// </summary>
public sealed class RectangleRoiGroup : DrawingObjectGroup
{
    private readonly RectangleShape _rect;
    private readonly LineShape _spineLine;
    private readonly RectangleShape _bodyHitbox;

    private readonly CircleShape _pointAHandle;
    private readonly CircleShape _pointAHitbox;
    private readonly CircleShape _pointBHandle;
    private readonly CircleShape _pointBHitbox;

    private readonly CircleShape[] _widthHandles  = new CircleShape[2]; // ZIndex 4, visible
    private readonly CircleShape[] _widthHitboxes = new CircleShape[2]; // ZIndex 5, invisible + IsMovable
    private readonly EventHandler<MovedEventArgs>[] _widthMovedHandlers = new EventHandler<MovedEventArgs>[2];

    private double _pointAX, _pointAY;
    private double _pointBX, _pointBY;
    private double _halfWidth;

    /// <summary>Initializes a new <see cref="RectangleRoiGroup"/> with the given spine endpoints and half-width.</summary>
    /// <param name="pointAX">World-space X coordinate of spine endpoint A.</param>
    /// <param name="pointAY">World-space Y coordinate of spine endpoint A.</param>
    /// <param name="pointBX">World-space X coordinate of spine endpoint B.</param>
    /// <param name="pointBY">World-space Y coordinate of spine endpoint B.</param>
    /// <param name="halfWidth">Half the width of the rectangle, measured perpendicularly to the spine.</param>
    public RectangleRoiGroup(double pointAX, double pointAY, double pointBX, double pointBY, double halfWidth)
    {
        _pointAX = pointAX;
        _pointAY = pointAY;
        _pointBX = pointBX;
        _pointBY = pointBY;
        _halfWidth = halfWidth;

        _rect = new RectangleShape
        {
            Stroke = new SolidColorBrush(Color.Parse("#3574F0")),
            StrokeThickness = 2.0,
            FillHover = new SolidColorBrush(Color.FromArgb(60, 53, 116, 240)),
            StrokeHover = new SolidColorBrush(Colors.White),
            ZIndex = 0
        };

        _spineLine = new LineShape
        {
            Stroke = new SolidColorBrush(Color.Parse("#3574F0")),
            StrokeThickness = 1.5,
            StrokeDashArray = [6, 4],
            ZIndex = 0
        };

        _bodyHitbox = new RectangleShape
        {
            IsMovable = true,
            Fill = null,
            Stroke = null,
            ZIndex = 1,
            Cursor = new Cursor(StandardCursorType.SizeAll)
        };

        _pointAHandle = new CircleShape
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

        _pointAHitbox = new CircleShape
        {
            IsFixedWidth = true,
            IsFixedHeight = true,
            Width = 25,
            Height = 25,
            IsMovable = true,
            Fill = null,
            Stroke = null,
            ZIndex = 3,
            Cursor = new Cursor(StandardCursorType.Cross)
        };

        _pointBHandle = new CircleShape
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

        _pointBHitbox = new CircleShape
        {
            IsFixedWidth = true,
            IsFixedHeight = true,
            Width = 25,
            Height = 25,
            IsMovable = true,
            Fill = null,
            Stroke = null,
            ZIndex = 3,
            Cursor = new Cursor(StandardCursorType.Cross)
        };

        // Create width handle shapes BEFORE any Items.Add — the base class fires RecalculateCoordinates()
        // on every CollectionChanged, so all shapes must be non-null before the first Add.
        for (int i = 0; i < 2; i++)
        {
            _widthHandles[i] = new CircleShape
            {
                IsFixedWidth  = true,
                IsFixedHeight = true,
                Width  = 12,
                Height = 12,
                Fill        = new SolidColorBrush(Colors.White),
                FillHover   = new SolidColorBrush(Color.Parse("#3574F0")),
                Stroke      = new SolidColorBrush(Color.Parse("#3574F0")),
                StrokeThickness = 1.5,
                ZIndex = 4
            };

            _widthHitboxes[i] = new CircleShape
            {
                IsFixedWidth  = true,
                IsFixedHeight = true,
                Width  = 25,
                Height = 25,
                IsMovable = true,
                Fill   = null,
                Stroke = null,
                ZIndex = 5,
                Cursor = new Cursor(StandardCursorType.Cross)
            };
        }

        Items.Add(_rect);
        Items.Add(_spineLine);
        Items.Add(_bodyHitbox);
        Items.Add(_pointAHandle);
        Items.Add(_pointAHitbox);
        Items.Add(_pointBHandle);
        Items.Add(_pointBHitbox);
        for (int i = 0; i < 2; i++)
        {
            Items.Add(_widthHandles[i]);
            Items.Add(_widthHitboxes[i]);
        }

        _bodyHitbox.Moved += OnBodyHitboxMoved;
        _pointAHitbox.Moved += OnPointAHitboxMoved;
        _pointBHitbox.Moved += OnPointBHitboxMoved;
        for (int i = 0; i < 2; i++)
        {
            var idx = i;
            _widthMovedHandlers[i] = (_, e) => OnWidthHitboxMoved(idx, e);
            _widthHitboxes[i].Moved += _widthMovedHandlers[i];
        }
    }

    /// <inheritdoc/>
    public override void RecalculateCoordinates()
    {
        var centerX = (_pointAX + _pointBX) / 2.0;
        var centerY = (_pointAY + _pointBY) / 2.0;

        var dx = _pointBX - _pointAX;
        var dy = _pointBY - _pointAY;
        var spineLen = Math.Max(1.0, Math.Sqrt(dx * dx + dy * dy));

        var spineDirX = dx / spineLen;
        var spineDirY = dy / spineLen;

        // A→B is local "down": (-sinR, cosR) → sinR = -spineDirX, cosR = spineDirY
        var sinR = -spineDirX;
        var cosR =  spineDirY;
        var rotation = Math.Atan2(sinR, cosR) * 180.0 / Math.PI;

        // Perpendicular "right" direction: (cosR, sinR)
        var perpX = cosR;
        var perpY = sinR;

        // Visual rectangle: top-left in unrotated local space is (cx - halfWidth, cy - spineLen/2)
        _rect.X = centerX - _halfWidth;
        _rect.Y = centerY - spineLen / 2.0;
        _rect.Width  = _halfWidth * 2.0;
        _rect.Height = spineLen;
        _rect.Rotation = rotation;

        // Body hitbox: same dimensions, positioned by center
        _bodyHitbox.Width    = _halfWidth * 2.0;
        _bodyHitbox.Height   = spineLen;
        _bodyHitbox.CenterX  = centerX;
        _bodyHitbox.CenterY  = centerY;
        _bodyHitbox.Rotation = rotation;

        // Spine line (decorative dashed A→B)
        _spineLine.X  = _pointAX;
        _spineLine.Y  = _pointAY;
        _spineLine.X2 = _pointBX;
        _spineLine.Y2 = _pointBY;

        // Endpoint handles
        _pointAHandle.CenterX  = _pointAX;
        _pointAHandle.CenterY  = _pointAY;
        _pointAHitbox.CenterX  = _pointAX;
        _pointAHitbox.CenterY  = _pointAY;

        _pointBHandle.CenterX  = _pointBX;
        _pointBHandle.CenterY  = _pointBY;
        _pointBHitbox.CenterX  = _pointBX;
        _pointBHitbox.CenterY  = _pointBY;

        // Width handles at spine midpoint ± halfWidth in the perpendicular direction
        _widthHandles[0].CenterX  = centerX + _halfWidth * perpX;
        _widthHandles[0].CenterY  = centerY + _halfWidth * perpY;
        _widthHitboxes[0].CenterX = centerX + _halfWidth * perpX;
        _widthHitboxes[0].CenterY = centerY + _halfWidth * perpY;

        _widthHandles[1].CenterX  = centerX - _halfWidth * perpX;
        _widthHandles[1].CenterY  = centerY - _halfWidth * perpY;
        _widthHitboxes[1].CenterX = centerX - _halfWidth * perpX;
        _widthHitboxes[1].CenterY = centerY - _halfWidth * perpY;
    }

    private void OnBodyHitboxMoved(object? sender, MovedEventArgs e)
    {
        _pointAX += e.DeltaX;
        _pointAY += e.DeltaY;
        _pointBX += e.DeltaX;
        _pointBY += e.DeltaY;
        RecalculateCoordinates();
    }

    private void OnPointAHitboxMoved(object? sender, MovedEventArgs e)
    {
        _pointAX += e.DeltaX;
        _pointAY += e.DeltaY;
        RecalculateCoordinates();
    }

    private void OnPointBHitboxMoved(object? sender, MovedEventArgs e)
    {
        _pointBX += e.DeltaX;
        _pointBY += e.DeltaY;
        RecalculateCoordinates();
    }

    private void OnWidthHitboxMoved(int index, MovedEventArgs e)
    {
        var dx = _pointBX - _pointAX;
        var dy = _pointBY - _pointAY;
        var spineLen = Math.Max(1.0, Math.Sqrt(dx * dx + dy * dy));

        var sinR = -dx / spineLen;
        var cosR =  dy / spineLen;

        // Perpendicular "right" direction: (cosR, sinR)
        var perpX = cosR;
        var perpY = sinR;

        var projected = e.DeltaX * perpX + e.DeltaY * perpY;
        if (index == 0)
            _halfWidth = Math.Max(5.0, _halfWidth + projected);
        else
            _halfWidth = Math.Max(5.0, _halfWidth - projected);

        RecalculateCoordinates();
    }

    /// <inheritdoc/>
    public override void UnregisterEvents()
    {
        _bodyHitbox.Moved  -= OnBodyHitboxMoved;
        _pointAHitbox.Moved -= OnPointAHitboxMoved;
        _pointBHitbox.Moved -= OnPointBHitboxMoved;
        for (int i = 0; i < 2; i++)
            _widthHitboxes[i].Moved -= _widthMovedHandlers[i];
        UnregisterCollectionEvents();
    }
}
