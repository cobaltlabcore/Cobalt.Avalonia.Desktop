using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cobalt.Avalonia.Desktop.Controls.CalendarSchedule;

/// <summary>Represents a single event or appointment displayed in a <see cref="CalendarSchedule"/>.</summary>
public class CalendarScheduleItem : ObservableObject
{
    /// <summary>Gets or sets the display title of the appointment.</summary>
    public string? Title
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the start time of the appointment.</summary>
    public DateTimeOffset Start
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the end time of the appointment.</summary>
    public DateTimeOffset End
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the background brush used to color-code the appointment.</summary>
    public IBrush? Color
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets an optional description for the appointment.</summary>
    public string? Description
    {
        get;
        set => SetProperty(ref field, value);
    }
}