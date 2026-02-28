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
    /// <summary>The shape currently being dragged, or <see langword="null"/> when no drag is active.</summary>
    private Shape? _dragging;

    /// <summary>The canvas-space pointer position recorded during the last move event, used to compute per-frame deltas.</summary>
    private Point _lastPos;

    /// <summary>Starts panning on middle-button press, or begins dragging the topmost movable shape under the left-button press position.</summary>
    /// <param name="e">The pointer pressed event data.</param>
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

    /// <summary>Continues any active pan and translates the dragged shape by the pointer delta divided by the current zoom.</summary>
    /// <param name="e">The pointer moved event data.</param>
    public override void OnMouseMove(PointerEventArgs e)
    {
        Pan_OnMouseMove(e);

        if (_dragging is null || Owner is null) return;

        var pos = e.GetPosition(Owner);
        var zoom = Owner.ZoomFactor;
        _dragging.Move((pos.X - _lastPos.X) / zoom, (pos.Y - _lastPos.Y) / zoom);
        _lastPos = pos;
    }

    /// <summary>Ends the active drag and stops any pan gesture.</summary>
    /// <param name="e">The pointer released event data.</param>
    public override void OnMouseUp(PointerReleasedEventArgs e)
    {
        _dragging = null;
        StopPan_OnMouseUp(e);
    }

    /// <inheritdoc/>
    public override void OnMouseWheel(PointerWheelEventArgs e) => Zoom_OnMouseWheel(e);

    /// <inheritdoc/>
    public override void OnMouseDoubleClick(TappedEventArgs e) => ZoomToFit_OnMouseDoubleClick(e);

    /// <summary>
    /// Finds the movable <see cref="Shape"/> with the highest <see cref="DrawingObject.ZIndex"/>
    /// that passes a hit-test at <paramref name="canvasPoint"/>.
    /// </summary>
    /// <param name="canvasPoint">The canvas-space point to test.</param>
    /// <returns>The best candidate shape, or <see langword="null"/> if none is found.</returns>
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
