using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Media;

namespace Cobalt.Avalonia.Desktop.Controls.Ribbon;

/// <summary>
/// A ribbon button that opens a drop-down popup containing a list of <see cref="RibbonMenuItem"/> items.
/// </summary>
public class RibbonDropDownButton : TemplatedControl
{
    private Popup? _popup;

    /// <summary>Defines the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<RibbonDropDownButton, string?>(nameof(Header));

    /// <summary>Defines the <see cref="IconData"/> property.</summary>
    public static readonly StyledProperty<Geometry?> IconDataProperty =
        AvaloniaProperty.Register<RibbonDropDownButton, Geometry?>(nameof(IconData));

    /// <summary>Defines the <see cref="IsDropDownOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<RibbonDropDownButton, bool>(
            nameof(IsDropDownOpen),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Gets the collection of menu items shown in the drop-down popup.</summary>
    [Content]
    public AvaloniaList<RibbonMenuItem> Items { get; } = new();

    /// <summary>Gets or sets the text label displayed beneath the button icon.</summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Gets or sets the icon geometry displayed on the button.</summary>
    public Geometry? IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the drop-down popup is currently open.</summary>
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>Finds the <c>PART_Popup</c> template part.</summary>
    /// <param name="e">The template applied event data.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _popup = e.NameScope.Find<Popup>("PART_Popup");
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        PseudoClasses.Add(":pressed");
        IsDropDownOpen = !IsDropDownOpen;
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PseudoClasses.Remove(":pressed");
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        PseudoClasses.Remove(":pressed");
    }
}
