using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using global::Avalonia.Media;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D;

/// <summary>
/// A 2D viewport control for rendering <see cref="DrawingObject"/> and <see cref="DrawingObjectGroup"/> instances
/// with support for pan, zoom, a background image, and a configurable pointer/keyboard interaction strategy.
/// </summary>
public class Displayer2D : TemplatedControl
{
    /// <summary>Defines the <see cref="DrawingObjects"/> property.</summary>
    public static readonly StyledProperty<ObservableCollection<DrawingObject>?> DrawingObjectsProperty =
        AvaloniaProperty.Register<Displayer2D, ObservableCollection<DrawingObject>?>(nameof(DrawingObjects));

    /// <summary>Defines the <see cref="DrawingObjectGroups"/> property.</summary>
    public static readonly StyledProperty<ObservableCollection<DrawingObjectGroup>?> DrawingObjectGroupsProperty =
        AvaloniaProperty.Register<Displayer2D, ObservableCollection<DrawingObjectGroup>?>(nameof(DrawingObjectGroups));

    /// <summary>Defines the <see cref="UserInteraction"/> property.</summary>
    public static readonly StyledProperty<UserInteraction?> UserInteractionProperty =
        AvaloniaProperty.Register<Displayer2D, UserInteraction?>(nameof(UserInteraction));

    /// <summary>Defines the <see cref="ZoomFactor"/> property.</summary>
    public static readonly StyledProperty<double> ZoomFactorProperty =
        AvaloniaProperty.Register<Displayer2D, double>(nameof(ZoomFactor), defaultValue: 1.0);

    /// <summary>Defines the <see cref="PanX"/> property.</summary>
    public static readonly StyledProperty<double> PanXProperty =
        AvaloniaProperty.Register<Displayer2D, double>(nameof(PanX), defaultValue: 0.0);

    /// <summary>Defines the <see cref="PanY"/> property.</summary>
    public static readonly StyledProperty<double> PanYProperty =
        AvaloniaProperty.Register<Displayer2D, double>(nameof(PanY), defaultValue: 0.0);

    /// <summary>Defines the <see cref="BackgroundImage"/> property.</summary>
    public static readonly StyledProperty<IImage?> BackgroundImageProperty =
        AvaloniaProperty.Register<Displayer2D, IImage?>(nameof(BackgroundImage));

    /// <summary>Defines the <see cref="WorldMousePosition"/> property.</summary>
    public static readonly DirectProperty<Displayer2D, Point?> WorldMousePositionProperty =
        AvaloniaProperty.RegisterDirect<Displayer2D, Point?>(
            nameof(WorldMousePosition),
            o => o.WorldMousePosition);

    /// <summary>Gets or sets the flat collection of drawing objects rendered on the canvas.</summary>
    public ObservableCollection<DrawingObject>? DrawingObjects
    {
        get => GetValue(DrawingObjectsProperty);
        set => SetValue(DrawingObjectsProperty, value);
    }

    /// <summary>Gets or sets the collection of drawing object groups whose items are rendered on the canvas.</summary>
    public ObservableCollection<DrawingObjectGroup>? DrawingObjectGroups
    {
        get => GetValue(DrawingObjectGroupsProperty);
        set => SetValue(DrawingObjectGroupsProperty, value);
    }

    /// <summary>Gets or sets the interaction strategy that handles pointer and keyboard events.</summary>
    public UserInteraction? UserInteraction
    {
        get => GetValue(UserInteractionProperty);
        set => SetValue(UserInteractionProperty, value);
    }

    /// <summary>Gets or sets the current zoom multiplier applied to the world space.</summary>
    public double ZoomFactor { get => GetValue(ZoomFactorProperty); set => SetValue(ZoomFactorProperty, value); }

    /// <summary>Gets or sets the horizontal pan offset in canvas pixels.</summary>
    public double PanX { get => GetValue(PanXProperty); set => SetValue(PanXProperty, value); }

    /// <summary>Gets or sets the vertical pan offset in canvas pixels.</summary>
    public double PanY { get => GetValue(PanYProperty); set => SetValue(PanYProperty, value); }

    /// <summary>Gets or sets the image rendered as the background behind all drawing objects.</summary>
    public IImage? BackgroundImage
    {
        get => GetValue(BackgroundImageProperty);
        set => SetValue(BackgroundImageProperty, value);
    }

    private Point? _worldMousePosition;

    /// <summary>Gets or sets the current mouse cursor position in world coordinates, or <see langword="null"/> when the cursor is outside the canvas.</summary>
    public Point? WorldMousePosition
    {
        get => _worldMousePosition;
        set => SetAndRaise(WorldMousePositionProperty, ref _worldMousePosition, value);
    }

    /// <summary>Converts a point from world space to canvas (screen) space using the current zoom and pan.</summary>
    /// <param name="worldPoint">The point in world coordinates.</param>
    /// <returns>The corresponding point in canvas pixels.</returns>
    public Point WorldToCanvas(Point worldPoint)
    {
        var zoom = ZoomFactor;
        return new Point(worldPoint.X * zoom + PanX, worldPoint.Y * zoom + PanY);
    }

    /// <summary>Converts a point from canvas (screen) space to world space using the current zoom and pan.</summary>
    /// <param name="canvasPoint">The point in canvas pixels.</param>
    /// <returns>The corresponding point in world coordinates.</returns>
    public Point CanvasToWorld(Point canvasPoint)
    {
        var zoom = ZoomFactor;
        if (zoom == 0.0) return canvasPoint;
        return new Point((canvasPoint.X - PanX) / zoom, (canvasPoint.Y - PanY) / zoom);
    }

    /// <summary>Adjusts <see cref="ZoomFactor"/> and pan so that <paramref name="worldBounds"/> fills the viewport with optional padding.</summary>
    /// <param name="worldBounds">The world-space rectangle to fit.</param>
    /// <param name="padding">The padding in canvas pixels to leave around the bounds.</param>
    public void ZoomToFit(Rect worldBounds, double padding = 20)
    {
        var viewWidth = Bounds.Width - padding * 2;
        var viewHeight = Bounds.Height - padding * 2;
        if (viewWidth <= 0 || viewHeight <= 0 || worldBounds.Width <= 0 || worldBounds.Height <= 0)
            return;

        var zoom = Math.Min(viewWidth / worldBounds.Width, viewHeight / worldBounds.Height);
        var worldCenterX = worldBounds.X + worldBounds.Width / 2;
        var worldCenterY = worldBounds.Y + worldBounds.Height / 2;

        ZoomFactor = zoom;
        PanX = Bounds.Width / 2 - worldCenterX * zoom;
        PanY = Bounds.Height / 2 - worldCenterY * zoom;
    }

    /// <summary>Adjusts zoom and pan so that the current <see cref="BackgroundImage"/> fills the viewport with optional padding.</summary>
    /// <param name="padding">The padding in canvas pixels to leave around the image.</param>
    public void ZoomToFit(double padding = 20)
    {
        var bgImage = BackgroundImage;
        if (bgImage is null) return;
        ZoomToFit(new Rect(0, 0, bgImage.Size.Width, bgImage.Size.Height), padding);
    }

    private Displayer2DCanvas? _canvas;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Detach from old canvas
        if (_canvas != null)
            _canvas.Owner = null;

        _canvas = e.NameScope.Find<Displayer2DCanvas>("PART_Canvas");

        if (_canvas != null)
            _canvas.Owner = this;

        if (UserInteraction != null)
            UserInteraction.Owner = this;

        // Detach old pointer/key events then reattach
        PointerPressed -= OnPointerPressed;
        PointerReleased -= OnPointerReleased;
        PointerMoved -= OnPointerMoved;
        PointerWheelChanged -= OnPointerWheelChanged;
        DoubleTapped -= OnDoubleTapped;
        KeyDown -= OnKeyDown;
        KeyUp -= OnKeyUp;

        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
        DoubleTapped += OnDoubleTapped;
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;

        InvalidateCanvas();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DrawingObjectsProperty)
        {
            if (change.OldValue is ObservableCollection<DrawingObject> oldObjects)
            {
                oldObjects.CollectionChanged -= OnDrawingObjectsCollectionChanged;
                foreach (var obj in oldObjects)
                    obj.PropertyChanged -= OnDrawingObjectPropertyChanged;
            }

            if (change.NewValue is ObservableCollection<DrawingObject> newObjects)
            {
                newObjects.CollectionChanged += OnDrawingObjectsCollectionChanged;
                foreach (var obj in newObjects)
                    obj.PropertyChanged += OnDrawingObjectPropertyChanged;
            }

            InvalidateCanvas();
        }
        else if (change.Property == UserInteractionProperty)
        {
            if (change.OldValue is UserInteraction oldInteraction)
                oldInteraction.Owner = null;
            if (change.NewValue is UserInteraction newInteraction)
                newInteraction.Owner = this;
        }
        else if (change.Property == BackgroundImageProperty
              || change.Property == ZoomFactorProperty
              || change.Property == PanXProperty
              || change.Property == PanYProperty)
        {
            InvalidateCanvas();
        }
        else if (change.Property == DrawingObjectGroupsProperty)
        {
            if (change.OldValue is ObservableCollection<DrawingObjectGroup> oldGroups)
            {
                oldGroups.CollectionChanged -= OnGroupsCollectionChanged;
                foreach (var group in oldGroups)
                {
                    group.PropertyChanged -= OnDrawingObjectPropertyChanged;
                    foreach (var item in group.Items)
                        item.PropertyChanged -= OnDrawingObjectPropertyChanged;
                }
            }

            if (change.NewValue is ObservableCollection<DrawingObjectGroup> newGroups)
            {
                newGroups.CollectionChanged += OnGroupsCollectionChanged;
                foreach (var group in newGroups)
                {
                    group.PropertyChanged += OnDrawingObjectPropertyChanged;
                    foreach (var item in group.Items)
                        item.PropertyChanged += OnDrawingObjectPropertyChanged;
                }
            }

            InvalidateCanvas();
        }
    }

    private void OnDrawingObjectsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (DrawingObject obj in e.OldItems)
                obj.PropertyChanged -= OnDrawingObjectPropertyChanged;
        }

        if (e.NewItems != null)
        {
            foreach (DrawingObject obj in e.NewItems)
                obj.PropertyChanged += OnDrawingObjectPropertyChanged;
        }

        InvalidateCanvas();
    }

    private void OnGroupsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (DrawingObjectGroup group in e.OldItems)
            {
                group.PropertyChanged -= OnDrawingObjectPropertyChanged;
                foreach (var item in group.Items)
                    item.PropertyChanged -= OnDrawingObjectPropertyChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (DrawingObjectGroup group in e.NewItems)
            {
                group.PropertyChanged += OnDrawingObjectPropertyChanged;
                foreach (var item in group.Items)
                    item.PropertyChanged += OnDrawingObjectPropertyChanged;
            }
        }

        InvalidateCanvas();
    }

    private void OnDrawingObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateCanvas();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        UserInteraction?.OnRenderSizeChanged(result);
        return result;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e) =>
        UserInteraction?.OnMouseDown(e);

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e) =>
        UserInteraction?.OnMouseUp(e);

    private void OnPointerMoved(object? sender, PointerEventArgs e) =>
        UserInteraction?.OnMouseMove(e);

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) =>
        UserInteraction?.OnMouseWheel(e);

    private void OnDoubleTapped(object? sender, TappedEventArgs e) =>
        UserInteraction?.OnMouseDoubleClick(e);

    private void OnKeyDown(object? sender, KeyEventArgs e) =>
        UserInteraction?.OnKeyDown(e);

    private void OnKeyUp(object? sender, KeyEventArgs e) =>
        UserInteraction?.OnKeyUp(e);

    /// <summary>Forces a repaint of the canvas.</summary>
    public void Refresh() => _canvas?.InvalidateVisual();

    private void InvalidateCanvas() => _canvas?.InvalidateVisual();
}
