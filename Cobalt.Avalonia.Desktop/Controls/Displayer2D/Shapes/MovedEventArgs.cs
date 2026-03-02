namespace Cobalt.Avalonia.Desktop.Controls.Displayer2D.Shapes;

/// <summary>Provides data for the <see cref="Shape.Moved"/> event.</summary>
public sealed class MovedEventArgs : EventArgs
{
    /// <summary>Gets the horizontal distance the shape was moved in world units.</summary>
    public double DeltaX { get; init; }

    /// <summary>Gets the vertical distance the shape was moved in world units.</summary>
    public double DeltaY { get; init; }

    /// <summary>Gets the new world-space X coordinate after the move.</summary>
    public double NewX { get; init; }

    /// <summary>Gets the new world-space Y coordinate after the move.</summary>
    public double NewY { get; init; }
}