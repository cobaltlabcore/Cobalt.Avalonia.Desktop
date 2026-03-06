namespace Cobalt.Avalonia.Desktop.Controls.CalendarSchedule;

/// <summary>Provides data for the <see cref="CalendarSchedule.ItemMoved"/> and <see cref="CalendarSchedule.ItemResized"/> events.</summary>
public class CalendarScheduleItemChangedEventArgs : EventArgs
{
    /// <summary>Gets the appointment that was moved or resized.</summary>
    public required CalendarScheduleItem Item { get; init; }

    /// <summary>Gets the original start time before the interaction.</summary>
    public DateTimeOffset OriginalStart { get; init; }

    /// <summary>Gets the original end time before the interaction.</summary>
    public DateTimeOffset OriginalEnd { get; init; }

    /// <summary>Gets the new start time after the interaction.</summary>
    public DateTimeOffset NewStart { get; init; }

    /// <summary>Gets the new end time after the interaction.</summary>
    public DateTimeOffset NewEnd { get; init; }
}