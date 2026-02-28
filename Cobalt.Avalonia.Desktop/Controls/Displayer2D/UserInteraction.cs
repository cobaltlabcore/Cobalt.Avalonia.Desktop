using Avalonia;
using Avalonia.Input;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D;

/// <summary>
/// Base class for all pointer and keyboard interaction strategies used by <see cref="Displayer2D"/>.
/// Subclass and override the virtual <c>OnMouse*</c> and <c>OnKey*</c> methods to implement custom behavior.
/// Built-in protected helpers provide reusable pan and zoom functionality.
/// </summary>
public class UserInteraction
{
    /// <summary>Gets the <see cref="Displayer2D"/> that owns this interaction. Set automatically when assigned to the displayer.</summary>
    public Displayer2D? Owner { get; internal set; }

    /// <summary>Called when a pointer button is pressed over the displayer.</summary>
    /// <param name="e">Pointer event data.</param>
    public virtual void OnMouseDown(PointerPressedEventArgs e) { }

    /// <summary>Called when a pointer button is released over the displayer.</summary>
    /// <param name="e">Pointer event data.</param>
    public virtual void OnMouseUp(PointerReleasedEventArgs e) { }

    /// <summary>Called when the pointer moves over the displayer.</summary>
    /// <param name="e">Pointer event data.</param>
    public virtual void OnMouseMove(PointerEventArgs e) { }

    /// <summary>Called when the mouse wheel is scrolled over the displayer.</summary>
    /// <param name="e">Pointer wheel event data.</param>
    public virtual void OnMouseWheel(PointerWheelEventArgs e) { }

    /// <summary>Called when a double-tap or double-click occurs over the displayer.</summary>
    /// <param name="e">Tapped event data.</param>
    public virtual void OnMouseDoubleClick(TappedEventArgs e) { }

    /// <summary>Called when a key is pressed while the displayer has focus.</summary>
    /// <param name="e">Key event data.</param>
    public virtual void OnKeyDown(KeyEventArgs e) { }

    /// <summary>Called when a key is released while the displayer has focus.</summary>
    /// <param name="e">Key event data.</param>
    public virtual void OnKeyUp(KeyEventArgs e) { }

    /// <summary>Called when the displayer's rendered size changes.</summary>
    /// <param name="newSize">The new render size.</param>
    public virtual void OnRenderSizeChanged(global::Avalonia.Size newSize) { }

    private bool _isPanning;
    private Point _lastPoint;

    /// <summary>Begins a pan gesture by capturing the pointer and recording the current position.</summary>
    /// <param name="e">The mouse-down event that initiates the pan.</param>
    protected void StartPan_OnMouseDown(PointerPressedEventArgs e)
    {
        if (_isPanning)
            return;

        _isPanning = true;
        _lastPoint = e.GetPosition(Owner);
        e.Pointer.Capture(e.Source as IInputElement);
    }

    /// <summary>Ends the pan gesture and releases pointer capture.</summary>
    /// <param name="e">The mouse-up event that ends the pan.</param>
    protected void StopPan_OnMouseUp(PointerReleasedEventArgs e)
    {
        _isPanning = false;
        e.Pointer.Capture(null);
    }

    /// <summary>Applies pan movement based on the pointer delta since the last move event.</summary>
    /// <param name="e">The pointer-moved event.</param>
    protected void Pan_OnMouseMove(PointerEventArgs e)
    {
        if (!_isPanning || Owner is null) return;

        var pos = e.GetPosition(Owner);
        Owner.PanX += pos.X - _lastPoint.X;
        Owner.PanY += pos.Y - _lastPoint.Y;
        _lastPoint = pos;
    }

    /// <summary>Zooms in or out centered on the current pointer position.</summary>
    /// <param name="e">The pointer wheel event containing the scroll delta.</param>
    protected void Zoom_OnMouseWheel(PointerWheelEventArgs e)
    {
        if (Owner is null) return;

        var zoomDelta = e.Delta.Y > 0 ? 1.4 : 1.0 / 1.4;
        var pivot = e.GetPosition(Owner);
        var worldPivot = Owner.CanvasToWorld(pivot);
        var newZoom = Owner.ZoomFactor * zoomDelta;

        Owner.ZoomFactor = newZoom;
        Owner.PanX = pivot.X - worldPivot.X * newZoom;
        Owner.PanY = pivot.Y - worldPivot.Y * newZoom;
    }

    /// <summary>Resets zoom to 1× and clears pan offsets.</summary>
    /// <param name="e">The double-tap event.</param>
    protected void ResetZoom_OnMouseDoubleClick(TappedEventArgs e)
    {
        if (Owner is null) return;
        Owner.ZoomFactor = 1.0;
        Owner.PanX = 0;
        Owner.PanY = 0;
    }

    /// <summary>Zooms to fit the background image, or resets zoom if no background image is present.</summary>
    /// <param name="e">The double-tap event.</param>
    protected void ZoomToFit_OnMouseDoubleClick(TappedEventArgs e)
    {
        if (Owner is null) return;

        if (Owner.BackgroundImage is not null)
            Owner.ZoomToFit();
        else
            ResetZoom_OnMouseDoubleClick(e);
    }
}
