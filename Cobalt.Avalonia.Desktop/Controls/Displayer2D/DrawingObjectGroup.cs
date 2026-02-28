using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D;

/// <summary>
/// Abstract base class for logical groups that own a collection of <see cref="DrawingObject"/> instances
/// and automatically trigger coordinate recalculation when the collection changes.
/// </summary>
public abstract class DrawingObjectGroup : ObservableObject
{
    /// <summary>Gets the collection of drawing objects owned by this group.</summary>
    public ObservableCollection<DrawingObject> Items { get; } = new();

    /// <summary>Initializes a new instance and subscribes to collection change notifications.</summary>
    protected DrawingObjectGroup()
    {
        Items.CollectionChanged += OnItemsCollectionChanged;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RecalculateCoordinates();
    }

    /// <summary>Recomputes derived coordinates or positions for all shapes in this group.</summary>
    public abstract void RecalculateCoordinates();

    /// <summary>Unsubscribes all event handlers managed by this group to prevent memory leaks.</summary>
    public abstract void UnregisterEvents();

    /// <summary>Removes the collection-changed handler attached in the constructor.</summary>
    protected void UnregisterCollectionEvents()
    {
        Items.CollectionChanged -= OnItemsCollectionChanged;
    }
}
