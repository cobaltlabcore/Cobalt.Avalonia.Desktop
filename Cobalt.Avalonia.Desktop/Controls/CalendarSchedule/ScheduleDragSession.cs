using Avalonia.Controls;
using Avalonia;

namespace Cobalt.Avalonia.Desktop.Controls.CalendarSchedule;

/// <summary>Holds transient state for an in-progress appointment drag or resize gesture.</summary>
internal sealed class ScheduleDragSession
{
    /// <summary>Whether this session is moving or resizing the appointment.</summary>
    public ScheduleInteractionMode Mode { get; set; }

    /// <summary>The appointment being interacted with.</summary>
    public CalendarScheduleItem Item { get; set; } = default!;

    /// <summary>The <see cref="Border"/> visual representing the appointment.</summary>
    public Border Border { get; set; } = default!;

    /// <summary>The appointment's start time at the beginning of the drag, used to revert on cancel.</summary>
    public DateTimeOffset OriginalStart { get; set; }

    /// <summary>The appointment's end time at the beginning of the drag, used to revert on cancel.</summary>
    public DateTimeOffset OriginalEnd { get; set; }

    /// <summary>The grid-space pointer position when the drag was initiated, used to detect threshold.</summary>
    public Point StartPointerPosition { get; set; }

    /// <summary>The vertical offset from the top of the appointment border to the pointer, used to keep the grab point stable during a move.</summary>
    public double PointerToTopOffset { get; set; }

    /// <summary>The semi-transparent ghost border placed at the original position during a drag, or <see langword="null"/> before threshold is exceeded.</summary>
    public Border? Ghost { get; set; }

    /// <summary>Whether the pointer has moved beyond the drag threshold pixels from the starting position.</summary>
    public bool ThresholdExceeded { get; set; }

    /// <summary>The 0-based day column index of the appointment at the start of the drag.</summary>
    public int OriginalDayIndex { get; set; }
}