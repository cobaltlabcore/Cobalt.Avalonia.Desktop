using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using global::Avalonia.Media;
using Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D;

/// <summary>
/// Internal rendering surface used by <see cref="Displayer2D"/>.
/// Renders the background image and all visible <see cref="DrawingObject"/> instances sorted by
/// <see cref="DrawingObject.ZIndex"/>, and maintains hover state for <see cref="Shape"/> instances.
/// </summary>
internal sealed class Displayer2DCanvas : Control
{
    /// <summary>Gets or sets the <see cref="Displayer2D"/> that owns this canvas.</summary>
    internal Displayer2D? Owner { get; set; }

    public Displayer2DCanvas()
    {
        PointerMoved += OnPointerMoved;
        PointerExited += OnPointerExited;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Owner is null) return;

        // Clip to canvas bounds to prevent overflow artifacts during rapid zoom
        using var clip = context.PushClip(new global::Avalonia.Rect(Bounds.Size));

        var zoom = Owner.ZoomFactor;
        var panX = Owner.PanX;
        var panY = Owner.PanY;

        // Render background image at world origin, behind all drawing objects
        var bgImage = Owner.BackgroundImage;
        if (bgImage is not null)
        {
            using var transform = context.PushTransform(
                Matrix.CreateScale(zoom, zoom) * Matrix.CreateTranslation(panX, panY));
            context.DrawImage(bgImage, new global::Avalonia.Rect(bgImage.Size));
        }

        var objects = Enumerable.Empty<DrawingObject>();

        if (Owner.DrawingObjects != null)
            objects = objects.Concat(Owner.DrawingObjects);

        if (Owner.DrawingObjectGroups != null)
        {
            foreach (var group in Owner.DrawingObjectGroups)
                objects = objects.Concat(group.Items);
        }

        foreach (var obj in objects.OrderBy(o => o.ZIndex))
        {
            if (!obj.IsVisible) continue;
            obj.RecalculateCoordinates(zoom, panX, panY);
            obj.Render(context);
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var pos = e.GetPosition(this);
        UpdateHoverState(pos);
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        // Clear hover state for shapes but keep WorldMousePosition updating
        // (the view may continue tracking coordinates outside the control)
        if (Owner is null) return;

        bool changed = false;

        var objects = Enumerable.Empty<DrawingObject>();
        if (Owner.DrawingObjects != null)
            objects = objects.Concat(Owner.DrawingObjects);
        if (Owner.DrawingObjectGroups != null)
            foreach (var group in Owner.DrawingObjectGroups)
                objects = objects.Concat(group.Items);

        foreach (var obj in objects)
        {
            if (obj is not Shape shape) continue;
            if (shape.IsHovered)
            {
                shape.IsHovered = false;
                changed = true;
            }
        }

        if (changed) InvalidateVisual();
        Cursor = null;
    }

    private void UpdateHoverState(global::Avalonia.Point? pos)
    {
        if (Owner is null) return;

        Owner.WorldMousePosition = pos.HasValue ? Owner.CanvasToWorld(pos.Value) : null;

        bool changed = false;

        var objects = Enumerable.Empty<DrawingObject>();
        if (Owner.DrawingObjects != null)
            objects = objects.Concat(Owner.DrawingObjects);
        if (Owner.DrawingObjectGroups != null)
            foreach (var group in Owner.DrawingObjectGroups)
                objects = objects.Concat(group.Items);

        Cursor? newCursor = null;
        int cursorZIndex = int.MinValue;

        foreach (var obj in objects)
        {
            if (obj is not Shape shape) continue;
            bool shouldHover = pos.HasValue && shape.IsVisible && shape.HitTest(pos.Value);
            if (shape.IsHovered != shouldHover)
            {
                shape.IsHovered = shouldHover;
                changed = true;
            }

            if (shouldHover && shape.Cursor is not null && shape.ZIndex > cursorZIndex)
            {
                cursorZIndex = shape.ZIndex;
                newCursor = shape.Cursor;
            }
        }

        Cursor = newCursor;

        if (changed) InvalidateVisual();
    }
}
