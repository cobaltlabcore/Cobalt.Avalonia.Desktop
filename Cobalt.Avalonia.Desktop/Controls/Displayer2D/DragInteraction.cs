using Avalonia;
using Avalonia.Input;
using Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D;

/// <summary>
/// A <see cref="UserInteraction"/> that supports dragging movable <see cref="Shape"/> objects with the left
/// mouse button, panning with the middle button, zooming with the scroll wheel, and fitting to the background
/// image on double-click.
/// </summary>
public sealed class DragInteraction : UserInteraction
{
    private Shape? _dragging;
    private Point _lastPos;

    public override void OnMouseDown(PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(null).Properties;

        if (props.IsMiddleButtonPressed)
        {
            StartPan_OnMouseDown(e);
            return;
        }

        if (props.IsLeftButtonPressed && Owner is not null)
        {
            var canvasPoint = e.GetPosition(Owner);
            var target = FindDraggable(canvasPoint);
            if (target is not null)
            {
                _dragging = target;
                _lastPos = canvasPoint;
                e.Pointer.Capture(e.Source as IInputElement);
            }
        }
    }

    public override void OnMouseMove(PointerEventArgs e)
    {
        Pan_OnMouseMove(e);

        if (_dragging is null || Owner is null) return;

        var pos = e.GetPosition(Owner);
        var zoom = Owner.ZoomFactor;
        _dragging.Move((pos.X - _lastPos.X) / zoom, (pos.Y - _lastPos.Y) / zoom);
        _lastPos = pos;
    }

    public override void OnMouseUp(PointerReleasedEventArgs e)
    {
        _dragging = null;
        StopPan_OnMouseUp(e);
    }

    public override void OnMouseWheel(PointerWheelEventArgs e) => Zoom_OnMouseWheel(e);

    public override void OnMouseDoubleClick(TappedEventArgs e) => ZoomToFit_OnMouseDoubleClick(e);

    private Shape? FindDraggable(Point canvasPoint)
    {
        if (Owner is null) return null;

        Shape? best = null;
        int bestZIndex = int.MinValue;

        var objects = Enumerable.Empty<DrawingObject>();
        if (Owner.DrawingObjects != null)
            objects = objects.Concat(Owner.DrawingObjects);
        if (Owner.DrawingObjectGroups != null)
            foreach (var group in Owner.DrawingObjectGroups)
                objects = objects.Concat(group.Items);

        foreach (var obj in objects)
        {
            if (obj is Shape shape && shape.IsMovable && shape.HitTest(canvasPoint))
            {
                if (shape.ZIndex > bestZIndex)
                {
                    bestZIndex = shape.ZIndex;
                    best = shape;
                }
            }
        }

        return best;
    }
}
