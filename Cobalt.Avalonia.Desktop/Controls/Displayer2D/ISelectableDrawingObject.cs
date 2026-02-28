namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D;

/// <summary>Defines a drawing object that supports selection state and notifies when selection changes.</summary>
public interface ISelectableDrawingObject
{
    /// <summary>Gets or sets a value indicating whether this object is currently selected.</summary>
    bool IsSelected { get; set; }

    /// <summary>Raised when <see cref="IsSelected"/> changes.</summary>
    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
}

/// <summary>Provides data for the <see cref="ISelectableDrawingObject.SelectionChanged"/> event.</summary>
public sealed class SelectionChangedEventArgs : EventArgs
{
    /// <summary>Gets the new selection state.</summary>
    public bool IsSelected { get; init; }
}
