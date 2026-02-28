using Avalonia.Media;

namespace Cobalt.Avalonia.Desktop.Controls.CalendarSchedule;

/// <summary>Represents a single event or appointment displayed in a <see cref="CalendarSchedule"/>.</summary>
public class CalendarScheduleItem
{
    /// <summary>Gets or sets the display title of the appointment.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets the start time of the appointment.</summary>
    public DateTimeOffset Start { get; set; }

    /// <summary>Gets or sets the end time of the appointment.</summary>
    public DateTimeOffset End { get; set; }

    /// <summary>Gets or sets the background brush used to color-code the appointment.</summary>
    public IBrush? Color { get; set; }

    /// <summary>Gets or sets an optional description for the appointment.</summary>
    public string? Description { get; set; }
}

/// <summary>Specifies whether a <see cref="CalendarSchedule"/> displays a week or a full month.</summary>
public enum CalendarViewMode
{
    /// <summary>Displays a 7-day week view with hourly time slots.</summary>
    Week,

    /// <summary>Displays a full month grid with daily appointment summaries.</summary>
    Month
}

/// <summary>Specifies the current drag interaction mode for a calendar appointment.</summary>
public enum ScheduleInteractionMode
{
    /// <summary>No interaction is in progress.</summary>
    None,

    /// <summary>The appointment is being moved to a different time or day.</summary>
    Move,

    /// <summary>The appointment's start time is being dragged from the top edge.</summary>
    ResizeTop,

    /// <summary>The appointment's end time is being dragged from the bottom edge.</summary>
    ResizeBottom
}

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
