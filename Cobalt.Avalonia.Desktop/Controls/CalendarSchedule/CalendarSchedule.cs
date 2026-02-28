using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia;
using System.Globalization;

namespace Cobalt.Avalonia.Desktop.Controls.CalendarSchedule;

/// <summary>
/// A calendar control that displays appointments in either a month grid or a 7-day week view with hourly time slots.
/// Supports interactive drag-to-move and drag-to-resize of appointments in the week view.
/// </summary>
public class CalendarSchedule : TemplatedControl
{
    private const double HourHeight = 60.0;
    private const double DragThreshold = 5.0;
    private const double ResizeZonePixels = 6.0;
    private const double SnapMinutes = 15.0;
    private const double MinDurationMinutes = 15.0;
    private const double AutoScrollZonePixels = 30.0;
    private const double AutoScrollStep = 20.0;

    /// <summary>Raised when an appointment is moved to a new time or day via drag interaction in the week view.</summary>
    public event EventHandler<CalendarScheduleItemChangedEventArgs>? ItemMoved;

    /// <summary>Raised when an appointment's start or end time is changed via drag-resize in the week view.</summary>
    public event EventHandler<CalendarScheduleItemChangedEventArgs>? ItemResized;

    private sealed class ScheduleDragSession
    {
        public ScheduleInteractionMode Mode;
        public CalendarScheduleItem Item = null!;
        public Border Border = null!;
        public DateTimeOffset OriginalStart;
        public DateTimeOffset OriginalEnd;
        public Point StartPointerPosition;
        public double PointerToTopOffset;
        public Border? Ghost;
        public bool ThresholdExceeded;
        public int OriginalDayIndex;
    }

    private ScheduleDragSession? _dragSession;

    /// <summary>Defines the <see cref="DisplayDate"/> property.</summary>
    public static readonly StyledProperty<DateTimeOffset> DisplayDateProperty =
        AvaloniaProperty.Register<CalendarSchedule, DateTimeOffset>(
            nameof(DisplayDate),
            DateTimeOffset.Now,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the <see cref="ViewMode"/> property.</summary>
    public static readonly StyledProperty<CalendarViewMode> ViewModeProperty =
        AvaloniaProperty.Register<CalendarSchedule, CalendarViewMode>(
            nameof(ViewMode),
            CalendarViewMode.Month,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the <see cref="Items"/> property.</summary>
    public static readonly StyledProperty<IEnumerable<CalendarScheduleItem>?> ItemsProperty =
        AvaloniaProperty.Register<CalendarSchedule, IEnumerable<CalendarScheduleItem>?>(nameof(Items));

    /// <summary>Defines the <see cref="SelectedDate"/> property.</summary>
    public static readonly StyledProperty<DateTimeOffset?> SelectedDateProperty =
        AvaloniaProperty.Register<CalendarSchedule, DateTimeOffset?>(
            nameof(SelectedDate),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the <see cref="SelectedItem"/> property.</summary>
    public static readonly StyledProperty<CalendarScheduleItem?> SelectedItemProperty =
        AvaloniaProperty.Register<CalendarSchedule, CalendarScheduleItem?>(
            nameof(SelectedItem),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the <see cref="FirstDayOfWeek"/> property.</summary>
    public static readonly StyledProperty<DayOfWeek> FirstDayOfWeekProperty =
        AvaloniaProperty.Register<CalendarSchedule, DayOfWeek>(
            nameof(FirstDayOfWeek),
            DayOfWeek.Monday);

    /// <summary>Gets or sets the date that determines which month or week is currently displayed.</summary>
    public DateTimeOffset DisplayDate
    {
        get => GetValue(DisplayDateProperty);
        set => SetValue(DisplayDateProperty, value);
    }

    /// <summary>Gets or sets whether the calendar shows a month grid or a week time-slot view.</summary>
    public CalendarViewMode ViewMode
    {
        get => GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
    }

    /// <summary>Gets or sets the collection of appointments to display.</summary>
    public IEnumerable<CalendarScheduleItem>? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>Gets or sets the currently selected date, highlighted in both the main view and the mini calendar.</summary>
    public DateTimeOffset? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    /// <summary>Gets or sets the currently selected appointment.</summary>
    public CalendarScheduleItem? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Gets or sets the day of the week shown in the leftmost column of the calendar grid.</summary>
    public DayOfWeek FirstDayOfWeek
    {
        get => GetValue(FirstDayOfWeekProperty);
        set => SetValue(FirstDayOfWeekProperty, value);
    }

    // Template parts - Header
    private Button? _previousButton;
    private Button? _nextButton;
    private Button? _todayButton;
    private TextBlock? _headerTitle;
    private Button? _weekButton;
    private Button? _monthButton;

    // Template parts - Mini Calendar
    private Button? _miniCalPrevButton;
    private Button? _miniCalNextButton;
    private TextBlock? _miniCalTitle;
    private Grid? _miniCalGrid;

    // Template parts - Month View
    private Grid? _monthViewDayHeaders;
    private Grid? _monthViewGrid;

    // Template parts - Week View
    private Grid? _weekViewDayHeaders;
    private ScrollViewer? _weekViewScrollViewer;
    private Grid? _weekViewTimeGrid;

    // Template parts - View containers
    private Control? _monthView;
    private Control? _weekView;

    private DateTimeOffset _miniCalDisplayMonth;
    private bool _initialScrollDone;

    private readonly List<(Border border, CalendarScheduleItem item, IBrush baseBrush)> _appointmentBorders = new();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Header
        _previousButton = e.NameScope.Find<Button>("PART_PreviousButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextButton");
        _todayButton = e.NameScope.Find<Button>("PART_TodayButton");
        _headerTitle = e.NameScope.Find<TextBlock>("PART_HeaderTitle");
        _weekButton = e.NameScope.Find<Button>("PART_WeekButton");
        _monthButton = e.NameScope.Find<Button>("PART_MonthButton");

        // Mini Calendar
        _miniCalPrevButton = e.NameScope.Find<Button>("PART_MiniCalPrevButton");
        _miniCalNextButton = e.NameScope.Find<Button>("PART_MiniCalNextButton");
        _miniCalTitle = e.NameScope.Find<TextBlock>("PART_MiniCalTitle");
        _miniCalGrid = e.NameScope.Find<Grid>("PART_MiniCalGrid");

        // Month View
        _monthViewDayHeaders = e.NameScope.Find<Grid>("PART_MonthViewDayHeaders");
        _monthViewGrid = e.NameScope.Find<Grid>("PART_MonthViewGrid");

        // Week View
        _weekViewDayHeaders = e.NameScope.Find<Grid>("PART_WeekViewDayHeaders");
        _weekViewScrollViewer = e.NameScope.Find<ScrollViewer>("PART_WeekViewScrollViewer");
        _weekViewTimeGrid = e.NameScope.Find<Grid>("PART_WeekViewTimeGrid");

        // View containers
        _monthView = e.NameScope.Find<Control>("PART_MonthView");
        _weekView = e.NameScope.Find<Control>("PART_WeekView");

        // Wire events
        if (_previousButton != null) _previousButton.Click += (_, _) => NavigatePrevious();
        if (_nextButton != null) _nextButton.Click += (_, _) => NavigateNext();
        if (_todayButton != null) _todayButton.Click += (_, _) => NavigateToday();
        if (_weekButton != null) _weekButton.Click += (_, _) => ViewMode = CalendarViewMode.Week;
        if (_monthButton != null) _monthButton.Click += (_, _) => ViewMode = CalendarViewMode.Month;
        if (_miniCalPrevButton != null) _miniCalPrevButton.Click += (_, _) => MiniCalNavigate(-1);
        if (_miniCalNextButton != null) _miniCalNextButton.Click += (_, _) => MiniCalNavigate(1);

        if (_weekViewTimeGrid != null)
        {
            _weekViewTimeGrid.Background = Brushes.Transparent;
            _weekViewTimeGrid.PointerPressed += OnWeekGridPointerPressed;
            _weekViewTimeGrid.PointerMoved += OnWeekGridPointerMoved;
            _weekViewTimeGrid.PointerReleased += OnWeekGridPointerReleased;
            _weekViewTimeGrid.PointerCaptureLost += OnWeekGridPointerCaptureLost;
        }

        KeyDown += OnScheduleKeyDown;

        _miniCalDisplayMonth = new DateTimeOffset(DisplayDate.Year, DisplayDate.Month, 1, 0, 0, 0, DisplayDate.Offset);
        _initialScrollDone = false;

        UpdatePseudoClasses();
        Rebuild();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DisplayDateProperty ||
            change.Property == ItemsProperty ||
            change.Property == FirstDayOfWeekProperty)
        {
            CancelDrag();
            if (change.Property == DisplayDateProperty)
            {
                _miniCalDisplayMonth = new DateTimeOffset(DisplayDate.Year, DisplayDate.Month, 1, 0, 0, 0, DisplayDate.Offset);
                _initialScrollDone = false;
            }
            Rebuild();
        }
        else if (change.Property == ViewModeProperty)
        {
            CancelDrag();
            UpdatePseudoClasses();
            _initialScrollDone = false;
            Rebuild();
        }
        else if (change.Property == SelectedItemProperty)
        {
            UpdateAppointmentSelection();
        }
        else if (change.Property == SelectedDateProperty)
        {
            Rebuild();
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":week", ViewMode == CalendarViewMode.Week);
        PseudoClasses.Set(":month", ViewMode == CalendarViewMode.Month);
    }

    private void NavigatePrevious()
    {
        DisplayDate = ViewMode == CalendarViewMode.Month
            ? DisplayDate.AddMonths(-1)
            : DisplayDate.AddDays(-7);
    }

    private void NavigateNext()
    {
        DisplayDate = ViewMode == CalendarViewMode.Month
            ? DisplayDate.AddMonths(1)
            : DisplayDate.AddDays(7);
    }

    private void NavigateToday()
    {
        DisplayDate = DateTimeOffset.Now;
        SelectedDate = DateTimeOffset.Now;
    }

    private void MiniCalNavigate(int monthDelta)
    {
        _miniCalDisplayMonth = _miniCalDisplayMonth.AddMonths(monthDelta);
        UpdateMiniCalendar();
    }

    private void Rebuild()
    {
        if (_headerTitle == null) return;

        _appointmentBorders.Clear();

        UpdateHeader();
        UpdateMiniCalendar();

        if (ViewMode == CalendarViewMode.Month)
            UpdateMonthView();
        else
            UpdateWeekView();
    }

    private void UpdateHeader()
    {
        if (_headerTitle == null) return;

        if (ViewMode == CalendarViewMode.Month)
        {
            _headerTitle.Text = DisplayDate.ToString("MMMM yyyy");
        }
        else
        {
            var weekStart = GetWeekStart(DisplayDate);
            var weekEnd = weekStart.AddDays(6);
            if (weekStart.Month == weekEnd.Month)
                _headerTitle.Text = $"{weekStart:MMMM d} – {weekEnd:d}, {weekEnd:yyyy}";
            else if (weekStart.Year == weekEnd.Year)
                _headerTitle.Text = $"{weekStart:MMM d} – {weekEnd:MMM d}, {weekEnd:yyyy}";
            else
                _headerTitle.Text = $"{weekStart:MMM d, yyyy} – {weekEnd:MMM d, yyyy}";
        }
    }

    private void UpdateMiniCalendar()
    {
        if (_miniCalGrid == null || _miniCalTitle == null) return;

        _miniCalTitle.Text = _miniCalDisplayMonth.ToString("MMMM yyyy");

        _miniCalGrid.Children.Clear();

        // Day-of-week headers
        var dayNames = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
        for (int i = 0; i < 7; i++)
        {
            int dayIndex = ((int)FirstDayOfWeek + i) % 7;
            var header = new TextBlock
            {
                Text = dayNames[dayIndex][..2],
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 11,
                Foreground = GetBrush("CobaltForegroundSecondaryBrush")
            };
            Grid.SetColumn(header, i);
            Grid.SetRow(header, 0);
            _miniCalGrid.Children.Add(header);
        }

        // Calculate days
        var firstOfMonth = new DateTimeOffset(_miniCalDisplayMonth.Year, _miniCalDisplayMonth.Month, 1, 0, 0, 0, _miniCalDisplayMonth.Offset);
        int startDayOffset = (((int)firstOfMonth.DayOfWeek - (int)FirstDayOfWeek) + 7) % 7;
        var gridStart = firstOfMonth.AddDays(-startDayOffset);

        var today = DateTimeOffset.Now.Date;

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                var date = gridStart.AddDays(row * 7 + col);
                bool isCurrentMonth = date.Month == _miniCalDisplayMonth.Month && date.Year == _miniCalDisplayMonth.Year;
                bool isToday = date.Date == today;
                bool isSelected = SelectedDate.HasValue && date.Date == SelectedDate.Value.Date;

                var dayButton = new Button
                {
                    Content = date.Day.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(0),
                    MinWidth = 0,
                    MinHeight = 0,
                    FontSize = 11,
                    Background = isToday ? GetBrush("CobaltCalendarTodayBrush")
                               : isSelected ? GetBrush("CobaltCalendarSelectedBrush")
                               : Brushes.Transparent,
                    Foreground = !isCurrentMonth ? GetBrush("CobaltCalendarOutOfMonthBrush")
                               : isToday ? GetBrush("CobaltAccentBrush")
                               : GetBrush("CobaltForegroundBrush"),
                    BorderThickness = new Thickness(0),
                    CornerRadius = new CornerRadius(4)
                };

                var capturedDate = date;
                dayButton.Click += (_, _) =>
                {
                    SelectedDate = capturedDate;
                    DisplayDate = capturedDate;
                };

                Grid.SetColumn(dayButton, col);
                Grid.SetRow(dayButton, row + 1);
                _miniCalGrid.Children.Add(dayButton);
            }
        }
    }

    private void UpdateMonthView()
    {
        if (_monthViewDayHeaders == null || _monthViewGrid == null) return;

        // Update day headers
        _monthViewDayHeaders.Children.Clear();
        var dayNames = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
        for (int i = 0; i < 7; i++)
        {
            int dayIndex = ((int)FirstDayOfWeek + i) % 7;
            var header = new TextBlock
            {
                Text = dayNames[dayIndex],
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                Foreground = GetBrush("CobaltForegroundSecondaryBrush"),
                Margin = new Thickness(0, 0, 0, 4)
            };
            Grid.SetColumn(header, i);
            _monthViewDayHeaders.Children.Add(header);
        }

        // Calculate grid days
        var firstOfMonth = new DateTimeOffset(DisplayDate.Year, DisplayDate.Month, 1, 0, 0, 0, DisplayDate.Offset);
        int startDayOffset = (((int)firstOfMonth.DayOfWeek - (int)FirstDayOfWeek) + 7) % 7;
        var gridStart = firstOfMonth.AddDays(-startDayOffset);

        var today = DateTimeOffset.Now.Date;
        var items = Items?.ToList() ?? new List<CalendarScheduleItem>();

        _monthViewGrid.Children.Clear();

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                var date = gridStart.AddDays(row * 7 + col);
                bool isCurrentMonth = date.Month == DisplayDate.Month && date.Year == DisplayDate.Year;
                bool isToday = date.Date == today;
                bool isSelected = SelectedDate.HasValue && date.Date == SelectedDate.Value.Date;

                var cellContent = new StackPanel { Spacing = 1 };

                // Day number
                var dayNumber = new TextBlock
                {
                    Text = date.Day.ToString(),
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(4, 2),
                    Foreground = !isCurrentMonth ? GetBrush("CobaltCalendarOutOfMonthBrush")
                               : isToday ? GetBrush("CobaltAccentBrush")
                               : GetBrush("CobaltForegroundBrush"),
                    FontWeight = isToday ? FontWeight.Bold : FontWeight.Normal
                };
                cellContent.Children.Add(dayNumber);

                // Appointments for this day
                var dayItems = items.Where(item =>
                    item.Start.Date <= date.Date && item.End.Date >= date.Date)
                    .Take(3)
                    .ToList();

                foreach (var item in dayItems)
                {
                    var appointmentBorder = new Border
                    {
                        Background = item.Color ?? GetBrush("CobaltCalendarAppointmentBrush"),
                        CornerRadius = new CornerRadius(2),
                        Padding = new Thickness(3, 1),
                        Margin = new Thickness(2, 0),
                        Cursor = new Cursor(StandardCursorType.Hand)
                    };

                    var appointmentText = new TextBlock
                    {
                        Text = item.Title ?? "",
                        FontSize = 10,
                        Foreground = Brushes.White,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        MaxLines = 1
                    };
                    appointmentBorder.Child = appointmentText;

                    SetupAppointmentInteraction(appointmentBorder, item);

                    cellContent.Children.Add(appointmentBorder);
                }

                // More items indicator
                var totalDayItems = items.Count(item =>
                    item.Start.Date <= date.Date && item.End.Date >= date.Date);
                if (totalDayItems > 3)
                {
                    var moreText = new TextBlock
                    {
                        Text = $"+{totalDayItems - 3} more",
                        FontSize = 10,
                        Foreground = GetBrush("CobaltForegroundSecondaryBrush"),
                        Margin = new Thickness(4, 0)
                    };
                    cellContent.Children.Add(moreText);
                }

                var cellBorder = new Border
                {
                    BorderBrush = GetBrush("CobaltCalendarGridLineBrush"),
                    BorderThickness = new Thickness(0, 0, col < 6 ? 1 : 0, row < 5 ? 1 : 0),
                    Background = isToday ? GetBrush("CobaltCalendarTodayBrush")
                               : isSelected ? GetBrush("CobaltCalendarSelectedBrush")
                               : Brushes.Transparent,
                    Padding = new Thickness(2),
                    Child = cellContent
                };

                var capturedDate = date;
                cellBorder.PointerPressed += (_, _) =>
                {
                    SelectedDate = capturedDate;
                    SelectedItem = null;
                };

                Grid.SetColumn(cellBorder, col);
                Grid.SetRow(cellBorder, row);
                _monthViewGrid.Children.Add(cellBorder);
            }
        }
    }

    private void UpdateWeekView()
    {
        if (_weekViewDayHeaders == null || _weekViewTimeGrid == null) return;

        var weekStart = GetWeekStart(DisplayDate);
        var today = DateTimeOffset.Now.Date;
        var items = Items?.ToList() ?? new List<CalendarScheduleItem>();

        // Update day headers
        _weekViewDayHeaders.Children.Clear();

        // Empty cell above time column
        var emptyHeader = new TextBlock();
        Grid.SetColumn(emptyHeader, 0);
        _weekViewDayHeaders.Children.Add(emptyHeader);

        for (int i = 0; i < 7; i++)
        {
            var date = weekStart.AddDays(i);
            bool isToday = date.Date == today;

            var headerPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var dayName = new TextBlock
            {
                Text = date.ToString("ddd").ToUpperInvariant(),
                FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = isToday ? GetBrush("CobaltAccentBrush") : GetBrush("CobaltForegroundSecondaryBrush")
            };
            headerPanel.Children.Add(dayName);

            var dayNum = new TextBlock
            {
                Text = date.Day.ToString(),
                FontSize = 18,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = isToday ? GetBrush("CobaltAccentBrush") : GetBrush("CobaltForegroundBrush")
            };
            headerPanel.Children.Add(dayNum);

            Grid.SetColumn(headerPanel, i + 1);
            _weekViewDayHeaders.Children.Add(headerPanel);
        }

        // Build time grid
        _weekViewTimeGrid.Children.Clear();
        _weekViewTimeGrid.RowDefinitions.Clear();

        for (int hour = 0; hour < 24; hour++)
        {
            _weekViewTimeGrid.RowDefinitions.Add(new RowDefinition(HourHeight, GridUnitType.Pixel));
        }

        // Vertical day separators (full-height lines between columns)
        for (int col = 1; col < 7; col++)
        {
            var separator = new Border
            {
                Width = 1,
                Background = GetBrush("CobaltCalendarGridLineBrush"),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(separator, col);
            Grid.SetRow(separator, 0);
            Grid.SetRowSpan(separator, 24);
            _weekViewTimeGrid.Children.Add(separator);
        }

        // Time labels and hour lines
        for (int hour = 0; hour < 24; hour++)
        {
            // Time label
            var timeLabel = new TextBlock
            {
                Text = new TimeOnly(hour, 0).ToString("HH:mm"),
                FontSize = 11,
                Foreground = GetBrush("CobaltForegroundSecondaryBrush"),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(4, -6, 8, 0)
            };
            Grid.SetColumn(timeLabel, 0);
            Grid.SetRow(timeLabel, hour);
            _weekViewTimeGrid.Children.Add(timeLabel);

            // Horizontal line across day columns
            for (int col = 1; col <= 7; col++)
            {
                var line = new Border
                {
                    BorderBrush = GetBrush("CobaltCalendarGridLineBrush"),
                    BorderThickness = new Thickness(0, 1, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };
                Grid.SetColumn(line, col);
                Grid.SetRow(line, hour);
                _weekViewTimeGrid.Children.Add(line);
            }
        }

        // Place appointment items
        for (int dayIdx = 0; dayIdx < 7; dayIdx++)
        {
            var date = weekStart.AddDays(dayIdx);
            var dayItems = items.Where(item =>
                item.Start.Date <= date.Date && item.End.Date >= date.Date)
                .OrderBy(item => item.Start)
                .ToList();

            foreach (var item in dayItems)
            {
                // Calculate position
                var itemStart = item.Start.Date < date.Date
                    ? new TimeSpan(0, 0, 0)
                    : item.Start.TimeOfDay;
                var itemEnd = item.End.Date > date.Date
                    ? new TimeSpan(24, 0, 0)
                    : item.End.TimeOfDay;

                double topOffset = itemStart.TotalHours * HourHeight;
                double height = Math.Max((itemEnd - itemStart).TotalHours * HourHeight, 20);

                int startRow = (int)itemStart.TotalHours;
                if (startRow >= 24) startRow = 23;

                var appointmentBorder = new Border
                {
                    Background = item.Color ?? GetBrush("CobaltCalendarAppointmentBrush"),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6, 4),
                    Margin = new Thickness(2, topOffset - (startRow * HourHeight), 2, 0),
                    Height = height,
                    VerticalAlignment = VerticalAlignment.Top,
                    Cursor = new Cursor(StandardCursorType.Hand),
                    ClipToBounds = true
                };

                var textPanel = new StackPanel();
                var titleText = new TextBlock
                {
                    Text = item.Title ?? "",
                    FontSize = 11,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = Brushes.White,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                textPanel.Children.Add(titleText);

                if (height > 36)
                {
                    var timeText = new TextBlock
                    {
                        Text = $"{item.Start:HH:mm} – {item.End:HH:mm}",
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))
                    };
                    textPanel.Children.Add(timeText);
                }

                appointmentBorder.Child = textPanel;

                SetupAppointmentInteraction(appointmentBorder, item, isWeekView: true);

                Grid.SetColumn(appointmentBorder, dayIdx + 1);
                Grid.SetRow(appointmentBorder, startRow);
                _weekViewTimeGrid.Children.Add(appointmentBorder);
            }

            // Current time indicator
            if (date.Date == today)
            {
                var now = DateTimeOffset.Now;
                double nowOffset = now.TimeOfDay.TotalHours * HourHeight;
                int nowRow = (int)now.TimeOfDay.TotalHours;
                if (nowRow >= 24) nowRow = 23;

                var timeLine = new Border
                {
                    Height = 2,
                    Background = GetBrush("CobaltCalendarCurrentTimeBrush"),
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, nowOffset - (nowRow * HourHeight), 0, 0),
                    ZIndex = 10
                };
                Grid.SetColumn(timeLine, dayIdx + 1);
                Grid.SetRow(timeLine, nowRow);
                _weekViewTimeGrid.Children.Add(timeLine);
            }
        }

        // Scroll to 8:00 AM on initial build
        if (!_initialScrollDone && _weekViewScrollViewer != null)
        {
            _initialScrollDone = true;
            _weekViewScrollViewer.Offset = new Vector(0, 8 * HourHeight);
        }
    }

    private void SetupAppointmentInteraction(Border border, CalendarScheduleItem item, bool isWeekView = false)
    {
        var baseBrush = border.Background ?? GetBrush("CobaltCalendarAppointmentBrush");
        bool isSelected = SelectedItem == item;

        if (isSelected)
            border.BorderBrush = Brushes.White;

        border.BorderThickness = new Thickness(isSelected ? 1.5 : 0);

        _appointmentBorders.Add((border, item, baseBrush));

        border.PointerEntered += (_, _) =>
        {
            if (_dragSession == null)
                border.Opacity = 0.8;
        };

        border.PointerExited += (_, _) =>
        {
            if (_dragSession == null)
            {
                border.Opacity = 1.0;
                border.Cursor = new Cursor(StandardCursorType.Hand);
            }
        };

        if (isWeekView)
        {
            bool isMultiDay = item.Start.Date != item.End.Date;

            border.PointerMoved += (_, e) =>
            {
                if (_dragSession != null) return;
                if (isMultiDay) return;

                var pos = e.GetPosition(border);
                bool nearTop = pos.Y <= ResizeZonePixels && border.Bounds.Height >= 24;
                bool nearBottom = pos.Y >= border.Bounds.Height - ResizeZonePixels && border.Bounds.Height >= 24;

                border.Cursor = (nearTop || nearBottom)
                    ? new Cursor(StandardCursorType.SizeNorthSouth)
                    : new Cursor(StandardCursorType.Hand);
            };

            border.PointerPressed += (_, e) =>
            {
                SelectedItem = item;
                e.Handled = true;

                if (isMultiDay || _weekViewTimeGrid == null) return;

                var posOnBorder = e.GetPosition(border);
                bool nearTop = posOnBorder.Y <= ResizeZonePixels && border.Bounds.Height >= 24;
                bool nearBottom = posOnBorder.Y >= border.Bounds.Height - ResizeZonePixels && border.Bounds.Height >= 24;

                ScheduleInteractionMode mode;
                if (nearTop) mode = ScheduleInteractionMode.ResizeTop;
                else if (nearBottom) mode = ScheduleInteractionMode.ResizeBottom;
                else mode = ScheduleInteractionMode.Move;

                var posOnGrid = e.GetPosition(_weekViewTimeGrid);
                int dayIndex = PointerXToDayIndex(posOnGrid.X);

                _dragSession = new ScheduleDragSession
                {
                    Mode = mode,
                    Item = item,
                    Border = border,
                    OriginalStart = item.Start,
                    OriginalEnd = item.End,
                    StartPointerPosition = posOnGrid,
                    PointerToTopOffset = posOnBorder.Y,
                    OriginalDayIndex = dayIndex
                };

                e.Pointer.Capture(_weekViewTimeGrid);
            };
        }
        else
        {
            border.PointerPressed += (_, e) =>
            {
                SelectedItem = item;
                e.Handled = true;
            };
        }
    }

    private void UpdateAppointmentSelection()
    {
        foreach (var (border, item, _) in _appointmentBorders)
        {
            bool isSelected = SelectedItem == item;
            border.BorderBrush = isSelected ? Brushes.White : null;
            border.BorderThickness = new Thickness(isSelected ? 1.5 : 0);
        }
    }

    // --- Drag / Resize helpers ---

    private TimeSpan PointerYToTime(double y)
    {
        double hours = y / HourHeight;
        hours = Math.Clamp(hours, 0, 24);
        return TimeSpan.FromHours(hours);
    }

    private static TimeSpan SnapToInterval(TimeSpan time)
    {
        int totalMinutes = (int)Math.Round(time.TotalMinutes / SnapMinutes) * (int)SnapMinutes;
        totalMinutes = Math.Clamp(totalMinutes, 0, 24 * 60);
        return TimeSpan.FromMinutes(totalMinutes);
    }

    private int PointerXToDayIndex(double x)
    {
        if (_weekViewTimeGrid == null) return 0;

        // Column 0 is time labels; columns 1-7 are days
        var colDefs = _weekViewTimeGrid.ColumnDefinitions;
        if (colDefs.Count < 2) return 0;

        double timeLabelWidth = colDefs[0].ActualWidth;
        double dayAreaWidth = _weekViewTimeGrid.Bounds.Width - timeLabelWidth;
        if (dayAreaWidth <= 0) return 0;

        double dayColumnWidth = dayAreaWidth / 7.0;
        int index = (int)((x - timeLabelWidth) / dayColumnWidth);
        return Math.Clamp(index, 0, 6);
    }

    private void RepositionAppointmentBorder(Border border, TimeSpan start, TimeSpan end, int dayIndex)
    {
        double topOffset = start.TotalHours * HourHeight;
        double height = Math.Max((end - start).TotalHours * HourHeight, 20);

        int startRow = (int)start.TotalHours;
        if (startRow >= 24) startRow = 23;

        Grid.SetColumn(border, dayIndex + 1);
        Grid.SetRow(border, startRow);
        border.Margin = new Thickness(2, topOffset - (startRow * HourHeight), 2, 0);
        border.Height = height;
    }

    private void CreateDragGhost(ScheduleDragSession session)
    {
        if (session.Ghost != null || _weekViewTimeGrid == null) return;

        var start = session.OriginalStart.TimeOfDay;
        var end = session.OriginalEnd.TimeOfDay;
        double topOffset = start.TotalHours * HourHeight;
        double height = Math.Max((end - start).TotalHours * HourHeight, 20);
        int startRow = (int)start.TotalHours;
        if (startRow >= 24) startRow = 23;

        var ghost = new Border
        {
            Background = session.Border.Background,
            Opacity = 0.3,
            CornerRadius = new CornerRadius(4),
            Height = height,
            Margin = new Thickness(2, topOffset - (startRow * HourHeight), 2, 0),
            VerticalAlignment = VerticalAlignment.Top,
            IsHitTestVisible = false
        };

        Grid.SetColumn(ghost, session.OriginalDayIndex + 1);
        Grid.SetRow(ghost, startRow);
        _weekViewTimeGrid.Children.Add(ghost);
        session.Ghost = ghost;
    }

    private void RemoveDragGhost(ScheduleDragSession session)
    {
        if (session.Ghost != null && _weekViewTimeGrid != null)
        {
            _weekViewTimeGrid.Children.Remove(session.Ghost);
            session.Ghost = null;
        }
    }

    private void CancelDrag()
    {
        if (_dragSession == null) return;

        var session = _dragSession;
        _dragSession = null;

        // Revert item times
        session.Item.Start = session.OriginalStart;
        session.Item.End = session.OriginalEnd;

        // Revert border position
        var start = session.OriginalStart.TimeOfDay;
        var end = session.OriginalEnd.TimeOfDay;
        RepositionAppointmentBorder(session.Border, start, end, session.OriginalDayIndex);

        RemoveDragGhost(session);
        RefreshAppointmentContent(session.Border, session.Item);

        session.Border.Opacity = 1.0;
        Cursor = Cursor.Default;
    }

    private static void RefreshAppointmentContent(Border border, CalendarScheduleItem item)
    {
        if (border.Child is StackPanel panel && panel.Children.Count >= 2 &&
            panel.Children[1] is TextBlock timeText)
        {
            timeText.Text = $"{item.Start:HH:mm} – {item.End:HH:mm}";
        }
        else if (border.Child is StackPanel panel2 && panel2.Children.Count == 1 &&
                 border.Height > 36)
        {
            // Duration became long enough to show time text
            var newTimeText = new TextBlock
            {
                Text = $"{item.Start:HH:mm} – {item.End:HH:mm}",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))
            };
            panel2.Children.Add(newTimeText);
        }
    }

    // --- Week grid pointer event handlers ---

    private void OnWeekGridPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // If no drag session was started by an appointment border, this is a click on empty space
        if (_dragSession == null)
            SelectedItem = null;
    }

    private void OnWeekGridPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragSession == null || _weekViewTimeGrid == null) return;

        var session = _dragSession;
        var pos = e.GetPosition(_weekViewTimeGrid);

        // Check threshold
        if (!session.ThresholdExceeded)
        {
            var delta = pos - session.StartPointerPosition;
            if (Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y) < DragThreshold)
                return;

            session.ThresholdExceeded = true;
            CreateDragGhost(session);

            Cursor = session.Mode == ScheduleInteractionMode.Move
                ? new Cursor(StandardCursorType.SizeAll)
                : new Cursor(StandardCursorType.SizeNorthSouth);
        }

        var weekStart = GetWeekStart(DisplayDate);
        var duration = session.OriginalEnd - session.OriginalStart;

        switch (session.Mode)
        {
            case ScheduleInteractionMode.Move:
            {
                var rawTime = PointerYToTime(pos.Y - session.PointerToTopOffset);
                var snappedStart = SnapToInterval(rawTime);
                var snappedEnd = snappedStart + duration;

                // Clamp to 0-24h
                if (snappedEnd > TimeSpan.FromHours(24))
                {
                    snappedEnd = TimeSpan.FromHours(24);
                    snappedStart = snappedEnd - duration;
                }
                if (snappedStart < TimeSpan.Zero)
                {
                    snappedStart = TimeSpan.Zero;
                    snappedEnd = snappedStart + duration;
                }

                int dayIndex = PointerXToDayIndex(pos.X);
                var newDateBase = new DateTimeOffset(weekStart.AddDays(dayIndex).Date, session.OriginalStart.Offset);

                session.Item.Start = newDateBase + snappedStart;
                session.Item.End = newDateBase + snappedEnd;

                RepositionAppointmentBorder(session.Border, snappedStart, snappedEnd, dayIndex);
                RefreshAppointmentContent(session.Border, session.Item);
                break;
            }
            case ScheduleInteractionMode.ResizeTop:
            {
                var rawTime = PointerYToTime(pos.Y);
                var snappedStart = SnapToInterval(rawTime);
                var currentEnd = session.Item.End.TimeOfDay;

                // Enforce minimum duration
                if (currentEnd - snappedStart < TimeSpan.FromMinutes(MinDurationMinutes))
                    snappedStart = currentEnd - TimeSpan.FromMinutes(MinDurationMinutes);

                if (snappedStart < TimeSpan.Zero)
                    snappedStart = TimeSpan.Zero;

                int dayIndex = Grid.GetColumn(session.Border) - 1;
                var dateBase = new DateTimeOffset(session.Item.Start.Date, session.OriginalStart.Offset);

                session.Item.Start = dateBase + snappedStart;

                RepositionAppointmentBorder(session.Border, snappedStart, currentEnd, dayIndex);
                RefreshAppointmentContent(session.Border, session.Item);
                break;
            }
            case ScheduleInteractionMode.ResizeBottom:
            {
                var rawTime = PointerYToTime(pos.Y);
                var snappedEnd = SnapToInterval(rawTime);
                var currentStart = session.Item.Start.TimeOfDay;

                // Enforce minimum duration
                if (snappedEnd - currentStart < TimeSpan.FromMinutes(MinDurationMinutes))
                    snappedEnd = currentStart + TimeSpan.FromMinutes(MinDurationMinutes);

                if (snappedEnd > TimeSpan.FromHours(24))
                    snappedEnd = TimeSpan.FromHours(24);

                int dayIndex = Grid.GetColumn(session.Border) - 1;
                var dateBase = new DateTimeOffset(session.Item.End.Date, session.OriginalEnd.Offset);

                session.Item.End = dateBase + snappedEnd;

                RepositionAppointmentBorder(session.Border, currentStart, snappedEnd, dayIndex);
                RefreshAppointmentContent(session.Border, session.Item);
                break;
            }
        }

        // Auto-scroll when near edges of scroll viewer
        if (_weekViewScrollViewer != null)
        {
            var posInScroller = e.GetPosition(_weekViewScrollViewer);
            if (posInScroller.Y < AutoScrollZonePixels)
                _weekViewScrollViewer.Offset = new Vector(_weekViewScrollViewer.Offset.X,
                    Math.Max(0, _weekViewScrollViewer.Offset.Y - AutoScrollStep));
            else if (posInScroller.Y > _weekViewScrollViewer.Bounds.Height - AutoScrollZonePixels)
                _weekViewScrollViewer.Offset = new Vector(_weekViewScrollViewer.Offset.X,
                    _weekViewScrollViewer.Offset.Y + AutoScrollStep);
        }
    }

    private void OnWeekGridPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragSession == null) return;

        var session = _dragSession;
        _dragSession = null;

        e.Pointer.Capture(null);
        RemoveDragGhost(session);
        session.Border.Opacity = 1.0;
        Cursor = Cursor.Default;

        if (!session.ThresholdExceeded)
            return; // Was just a click — selection already handled

        var args = new CalendarScheduleItemChangedEventArgs
        {
            Item = session.Item,
            OriginalStart = session.OriginalStart,
            OriginalEnd = session.OriginalEnd,
            NewStart = session.Item.Start,
            NewEnd = session.Item.End
        };

        if (session.Mode == ScheduleInteractionMode.Move)
            ItemMoved?.Invoke(this, args);
        else
            ItemResized?.Invoke(this, args);
    }

    private void OnWeekGridPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        CancelDrag();
    }

    private void OnScheduleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _dragSession != null)
        {
            CancelDrag();
            e.Handled = true;
        }
    }

    private DateTimeOffset GetWeekStart(DateTimeOffset date)
    {
        int diff = (((int)date.DayOfWeek - (int)FirstDayOfWeek) + 7) % 7;
        return date.AddDays(-diff).Date;
    }

    private IBrush GetBrush(string resourceKey)
    {
        if (this.TryFindResource(resourceKey, ActualThemeVariant, out var resource) && resource is IBrush brush)
            return brush;
        return Brushes.Gray;
    }
}
