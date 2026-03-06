namespace Cobalt.Avalonia.Desktop.Controls.CalendarSchedule;

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