using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Cobalt.Avalonia.Desktop.Controls;
using Cobalt.Avalonia.Desktop.Controls.ContentDialog;
using Cobalt.Avalonia.Desktop.Controls.InfoBar;
using Cobalt.Avalonia.Desktop.Services;
using CobaltAvaloniaDesktopTester.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhosphorIconsAvalonia;

namespace CobaltAvaloniaDesktopTester.ViewModels;

public class ServicesTestingPageViewModel : ObservableObject
{
    private readonly IContentDialogService _dialogService;
    private readonly IOverlayService _overlayService;
    private readonly IInfoBarService _infoBarService;

    public ServicesTestingPageViewModel(
        IContentDialogService dialogService,
        IOverlayService overlayService,
        IInfoBarService infoBarService)
    {
        _dialogService = dialogService;
        _overlayService = overlayService;
        _infoBarService = infoBarService;

        // Dialog commands
        ShowSimpleDialogCommand = new AsyncRelayCommand(ShowSimpleDialog);
        ShowComplexDialogCommand = new AsyncRelayCommand(ShowComplexDialog);
        ShowPasswordDialogCommand = new AsyncRelayCommand(ShowPasswordDialog);

        // Overlay commands
        RunSimpleTaskCommand = new AsyncRelayCommand(RunSimpleTask, CanRunTask);
        RunComplexTaskCommand = new AsyncRelayCommand(RunComplexTask, CanRunTask);

        // InfoBar commands
        ShowInfoCommand = new AsyncRelayCommand(ShowInfo);
        ShowSuccessCommand = new AsyncRelayCommand(ShowSuccess);
        ShowWarningCommand = new AsyncRelayCommand(ShowWarning);
        ShowErrorCommand = new AsyncRelayCommand(ShowError);
        CloseInfoBarCommand = new AsyncRelayCommand(CloseInfoBar);
    }

    public string? LastDialogResult
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string? LastOverlayResult
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string? LastInfoBarResult
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsBusy
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value)) return;
            RunSimpleTaskCommand.NotifyCanExecuteChanged();
            RunComplexTaskCommand.NotifyCanExecuteChanged();
        }
    }

    // Dialog commands
    public IAsyncRelayCommand ShowSimpleDialogCommand { get; }
    public IAsyncRelayCommand ShowComplexDialogCommand { get; }
    public IAsyncRelayCommand ShowPasswordDialogCommand { get; }

    // Overlay commands
    public IAsyncRelayCommand RunSimpleTaskCommand { get; }
    public IAsyncRelayCommand RunComplexTaskCommand { get; }

    // InfoBar commands
    public IAsyncRelayCommand ShowInfoCommand { get; }
    public IAsyncRelayCommand ShowSuccessCommand { get; }
    public IAsyncRelayCommand ShowWarningCommand { get; }
    public IAsyncRelayCommand ShowErrorCommand { get; }
    public IAsyncRelayCommand CloseInfoBarCommand { get; }

    private bool CanRunTask() => !IsBusy;

    // Dialog methods
    private async Task ShowSimpleDialog()
    {
        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Information";
            dialog.Content = "This is a simple message dialog. Click OK to close it.";
            dialog.IconData = IconService.CreateGeometry(Icon.info, IconType.regular);
            dialog.PrimaryButtonText = "OK";
        });

        LastDialogResult = $"Simple dialog result: {result}";
    }

    private async Task ShowComplexDialog()
    {
        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Confirm Action";
            dialog.IconData = IconService.CreateGeometry(Icon.warning, IconType.regular);
            dialog.IconBrush = new SolidColorBrush(Color.Parse("#F59E0B"));
            dialog.Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Are you sure you want to perform this action?",
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = "This dialog demonstrates multiple buttons with different results.",
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(Color.Parse("#888888"))
                    }
                }
            };
            dialog.PrimaryButtonText = "Confirm";
            dialog.SecondaryButtonText = "Maybe Later";
            dialog.CloseButtonText = "Cancel";
        });

        LastDialogResult = $"Complex dialog result: {result}";
    }

    private async Task ShowPasswordDialog()
    {
        var passwordBox = new TextBox
        {
            PasswordChar = '\u2022',
            Watermark = "Enter your password",
            Width = 300
        };

        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Password Required";
            dialog.Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Please enter your password:",
                        TextWrapping = TextWrapping.Wrap
                    },
                    passwordBox
                }
            };
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
        });

        LastDialogResult = result == DialogResult.Primary
            ? $"Password entered: {passwordBox.Text}"
            : "Password dialog cancelled.";
    }

    // Overlay methods
    private async Task RunSimpleTask()
    {
        IsBusy = true;
        var sw = Stopwatch.StartNew();

        try
        {
            var card = new ProgressOverlayCard
            {
                Title = "Processing",
                IsIndeterminate = true,
                Message = "Initializing..."
            };

            await _overlayService.ShowAsync(card);
            await Task.Delay(2000);

            card.IsIndeterminate = false;
            card.Progress = 0;
            card.Message = "Step 1 of 4...";
            await Task.Delay(1000);

            card.Progress = 25;
            card.Message = "Step 2 of 4...";
            await Task.Delay(1000);

            card.Progress = 50;

            card.IsIndeterminate = true;
            card.Message = "Analyzing results...";
            await Task.Delay(2000);

            card.IsIndeterminate = false;
            card.Progress = 75;
            card.Message = "Step 3 of 4...";
            await Task.Delay(1000);

            card.Progress = 100;
            card.Message = "Step 4 of 4...";
            await Task.Delay(500);

            await _overlayService.HideAsync();
            sw.Stop();
            LastOverlayResult = $"Task completed in {sw.Elapsed.TotalSeconds:F1} seconds";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RunComplexTask()
    {
        IsBusy = true;
        var sw = Stopwatch.StartNew();

        var steps = new[]
        {
            "Validating input data",
            "Connecting to service",
            "Processing records",
            "Generating report",
            "Finalizing"
        };

        try
        {
            var card = new ProgressOverlayCard
            {
                Title = "Complex Operation",
                IsIndeterminate = true,
                Message = "Starting...",
                Content = BuildStepList(steps, -1)
            };

            await _overlayService.ShowAsync(card);

            for (int i = 0; i < steps.Length; i++)
            {
                card.IsIndeterminate = true;
                card.Message = steps[i] + "...";
                card.Content = BuildStepList(steps, i - 1);
                await Task.Delay(1500);

                int completed = i + 1;
                card.IsIndeterminate = false;
                card.Progress = (double)completed / steps.Length * 100;
                card.Content = BuildStepList(steps, i);
                await Task.Delay(500);
            }

            card.Message = "Done!";
            await Task.Delay(500);

            await _overlayService.HideAsync();
            sw.Stop();
            LastOverlayResult = $"Task completed in {sw.Elapsed.TotalSeconds:F1} seconds";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static StackPanel BuildStepList(string[] steps, int completedUpTo)
    {
        var panel = new StackPanel { Spacing = 4 };

        for (int i = 0; i < steps.Length; i++)
        {
            var prefix = i <= completedUpTo ? "\u2713 " : "  ";
            var tb = new TextBlock
            {
                Text = prefix + steps[i],
                Foreground = i <= completedUpTo
                    ? new SolidColorBrush(Color.Parse("#4CAF50"))
                    : new SolidColorBrush(Color.Parse("#888888"))
            };
            panel.Children.Add(tb);
        }

        return panel;
    }

    // InfoBar methods
    private async Task ShowInfo()
    {
        await _infoBarService.ShowAsync(o =>
        {
            o.Title = "Information";
            o.Message = "This is an informational message.";
            o.Severity = InfoBarSeverity.Info;
        });
        LastInfoBarResult = "Info closed";
    }

    private async Task ShowSuccess()
    {
        await _infoBarService.ShowAsync(o =>
        {
            o.Title = "Success";
            o.Message = "The operation completed successfully.";
            o.Severity = InfoBarSeverity.Success;
        });
        LastInfoBarResult = "Success closed";
    }

    private async Task ShowWarning()
    {
        await _infoBarService.ShowAsync(o =>
        {
            o.Title = "Warning";
            o.Message = "Something might need your attention.";
            o.Severity = InfoBarSeverity.Warning;
        });
        LastInfoBarResult = "Warning closed";
    }

    private async Task ShowError()
    {
        await _infoBarService.ShowAsync(o =>
        {
            o.Title = "Error";
            o.Message = "An error has occurred during the operation.";
            o.Severity = InfoBarSeverity.Error;
        });
        LastInfoBarResult = "Error closed";
    }

    private async Task CloseInfoBar()
    {
        await _infoBarService.HideAsync();
    }
}
