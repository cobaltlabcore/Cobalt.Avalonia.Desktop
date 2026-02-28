using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia;
using System.Windows.Input;

namespace Cobalt.Avalonia.Desktop.Controls;

/// <summary>
/// Specifies the result of a <see cref="ContentDialog"/> interaction.
/// </summary>
public enum DialogResult
{
    /// <summary>No button was pressed; the dialog was dismissed without a definitive choice.</summary>
    None,

    /// <summary>The primary action button was pressed.</summary>
    Primary,

    /// <summary>The secondary action button was pressed.</summary>
    Secondary,

    /// <summary>The close button was pressed.</summary>
    Close
}

/// <summary>
/// A modal dialog overlay that presents a title, content, and up to three action buttons.
/// Use <see cref="ShowAsync"/> to open the dialog and await a <see cref="DialogResult"/>.
/// </summary>
public class ContentDialog : ContentControl
{
    private Border? _overlayPart;

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(Title));

    /// <summary>Defines the <see cref="PrimaryButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> PrimaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(PrimaryButtonText));

    /// <summary>Defines the <see cref="SecondaryButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> SecondaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(SecondaryButtonText));

    /// <summary>Defines the <see cref="CloseButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> CloseButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(CloseButtonText));

    /// <summary>Defines the <see cref="PrimaryButtonCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> PrimaryButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(PrimaryButtonCommand));

    /// <summary>Defines the <see cref="SecondaryButtonCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> SecondaryButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(SecondaryButtonCommand));

    /// <summary>Defines the <see cref="CloseButtonCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> CloseButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(CloseButtonCommand));

    /// <summary>Defines the <see cref="IsPrimaryButtonEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsPrimaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsPrimaryButtonEnabled), true);

    /// <summary>Defines the <see cref="IsSecondaryButtonEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsSecondaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsSecondaryButtonEnabled), true);

    /// <summary>Defines the <see cref="IsCloseButtonEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsCloseButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsCloseButtonEnabled), true);

    /// <summary>Defines the <see cref="DefaultButton"/> property.</summary>
    public static readonly StyledProperty<DefaultButton> DefaultButtonProperty =
        AvaloniaProperty.Register<ContentDialog, DefaultButton>(nameof(DefaultButton), DefaultButton.None);

    /// <summary>Defines the <see cref="IsOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsOpen), false);

    /// <summary>Defines the <see cref="OverlayBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> OverlayBrushProperty =
        AvaloniaProperty.Register<ContentDialog, IBrush?>(
            nameof(OverlayBrush),
            new SolidColorBrush(Color.FromArgb(77, 0, 0, 0)));

    /// <summary>Defines the <see cref="DialogResult"/> property.</summary>
    public static readonly StyledProperty<DialogResult> DialogResultProperty =
        AvaloniaProperty.Register<ContentDialog, DialogResult>(nameof(DialogResult), DialogResult.None);

    /// <summary>Defines the <see cref="IconData"/> property.</summary>
    public static readonly StyledProperty<Geometry?> IconDataProperty =
        AvaloniaProperty.Register<ContentDialog, Geometry?>(nameof(IconData));

    /// <summary>Defines the <see cref="IconBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<ContentDialog, IBrush?>(nameof(IconBrush));

    /// <summary>Gets or sets the dialog title.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the text label for the primary action button.</summary>
    public string? PrimaryButtonText
    {
        get => GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Gets or sets the text label for the secondary action button.</summary>
    public string? SecondaryButtonText
    {
        get => GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Gets or sets the text label for the close button.</summary>
    public string? CloseButtonText
    {
        get => GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    /// <summary>Gets or sets the command executed when the primary button is clicked.</summary>
    public ICommand? PrimaryButtonCommand
    {
        get => GetValue(PrimaryButtonCommandProperty);
        set => SetValue(PrimaryButtonCommandProperty, value);
    }

    /// <summary>Gets or sets the command executed when the secondary button is clicked.</summary>
    public ICommand? SecondaryButtonCommand
    {
        get => GetValue(SecondaryButtonCommandProperty);
        set => SetValue(SecondaryButtonCommandProperty, value);
    }

    /// <summary>Gets or sets the command executed when the close button is clicked.</summary>
    public ICommand? CloseButtonCommand
    {
        get => GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the primary button is enabled.</summary>
    public bool IsPrimaryButtonEnabled
    {
        get => GetValue(IsPrimaryButtonEnabledProperty);
        set => SetValue(IsPrimaryButtonEnabledProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the secondary button is enabled.</summary>
    public bool IsSecondaryButtonEnabled
    {
        get => GetValue(IsSecondaryButtonEnabledProperty);
        set => SetValue(IsSecondaryButtonEnabledProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the close button is enabled.</summary>
    public bool IsCloseButtonEnabled
    {
        get => GetValue(IsCloseButtonEnabledProperty);
        set => SetValue(IsCloseButtonEnabledProperty, value);
    }

    /// <summary>Gets or sets which button is styled as the default (keyboard-focused) action.</summary>
    public DefaultButton DefaultButton
    {
        get => GetValue(DefaultButtonProperty);
        set => SetValue(DefaultButtonProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the dialog is currently visible.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Gets or sets the brush used to tint the background overlay behind the dialog.</summary>
    public IBrush? OverlayBrush
    {
        get => GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    /// <summary>Gets or sets the result of the last user interaction with the dialog.</summary>
    public DialogResult DialogResult
    {
        get => GetValue(DialogResultProperty);
        set => SetValue(DialogResultProperty, value);
    }

    /// <summary>Gets or sets the icon geometry displayed in the dialog title area.</summary>
    public Geometry? IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    /// <summary>Gets or sets the brush used to paint the dialog icon.</summary>
    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    /// <summary>Raised when the dialog is closed, with the <see cref="DialogResult"/> indicating which button was pressed.</summary>
    public event EventHandler<DialogResult>? Closed;

    /// <summary>
    /// Finds template parts for the overlay and all three buttons, and wires click and pointer handlers.
    /// </summary>
    /// <param name="e">The template applied event data.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _overlayPart = e.NameScope.Find<Border>("PART_Overlay");
        if (_overlayPart != null)
        {
            _overlayPart.PointerPressed += OnOverlayPointerPressed;
        }

        var primaryButton = e.NameScope.Find<Button>("PART_PrimaryButton");
        if (primaryButton != null)
        {
            primaryButton.Click += OnPrimaryButtonClick;
        }

        var secondaryButton = e.NameScope.Find<Button>("PART_SecondaryButton");
        if (secondaryButton != null)
        {
            secondaryButton.Click += OnSecondaryButtonClick;
        }

        var closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        if (closeButton != null)
        {
            closeButton.Click += OnCloseButtonClick;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape && IsOpen)
        {
            CloseDialog(DialogResult.None);
            e.Handled = true;
        }
    }

    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CloseDialog(DialogResult.None);
    }

    private void OnPrimaryButtonClick(object? sender, RoutedEventArgs e)
    {
        PrimaryButtonCommand?.Execute(null);
        CloseDialog(DialogResult.Primary);
    }

    private void OnSecondaryButtonClick(object? sender, RoutedEventArgs e)
    {
        SecondaryButtonCommand?.Execute(null);
        CloseDialog(DialogResult.Secondary);
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        CloseButtonCommand?.Execute(null);
        CloseDialog(DialogResult.Close);
    }

    private void CloseDialog(DialogResult result)
    {
        DialogResult = result;
        IsOpen = false;
        Closed?.Invoke(this, result);
    }

    /// <summary>Closes the dialog immediately, setting <see cref="DialogResult"/> to <see cref="DialogResult.None"/>.</summary>
    /// <returns>A task that completes when the dialog has closed.</returns>
    public Task HideAsync()
    {
        if (!IsOpen)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource();
        EventHandler<DialogResult>? handler = null;

        handler = (s, result) =>
        {
            Closed -= handler;
            tcs.SetResult();
        };

        Closed += handler;
        CloseDialog(DialogResult.None);

        return tcs.Task;
    }

    /// <summary>Opens the dialog and returns a task that resolves to the <see cref="DialogResult"/> when it closes.</summary>
    /// <returns>A task that completes with the <see cref="DialogResult"/> when the dialog is dismissed.</returns>
    public Task<DialogResult> ShowAsync()
    {
        var tcs = new TaskCompletionSource<DialogResult>();
        EventHandler<DialogResult>? handler = null;

        handler = (s, result) =>
        {
            Closed -= handler;
            tcs.SetResult(result);
        };

        Closed += handler;
        IsOpen = true;

        return tcs.Task;
    }
}

/// <summary>
/// Specifies which button in a <see cref="ContentDialog"/> is styled as the default action.
/// </summary>
// TODO: Should this enum be here ??
public enum DefaultButton
{
    /// <summary>No button is styled as the default.</summary>
    None,

    /// <summary>The primary button is styled as the default.</summary>
    Primary,

    /// <summary>The secondary button is styled as the default.</summary>
    Secondary,

    /// <summary>The close button is styled as the default.</summary>
    Close
}
