using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Cobalt.Avalonia.Desktop.Controls;
using Cobalt.Avalonia.Desktop.Services;
using CobaltAvaloniaDesktopTester.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace CobaltAvaloniaDesktopTester.ViewModels;

/// <summary>
/// Demonstrates navigation features including NavigateTo and navigation cancellation with INavigationLifecycleAsync.
/// </summary>
public class NavigationDemoPageViewModel : ObservableObject, INavigationViewModel
{
    private readonly IServiceProvider _services;
    private readonly INavigationService _navigation;
    private readonly IContentDialogService _dialogService;
    private string _documentText = string.Empty;
    private string _savedText = string.Empty;
    private bool _hasUnsavedChanges;

    public NavigationDemoPageViewModel(
        IServiceProvider services,
        INavigationService navigation,
        IContentDialogService dialogService)
    {
        _services = services;
        _navigation = navigation;
        _dialogService = dialogService;

        NavigateToSettingsCommand = new AsyncRelayCommand(NavigateToSettings);
        NavigateToDummyCommand    = new AsyncRelayCommand(NavigateToDummy);
        SaveCommand               = new RelayCommand(Save);
        DiscardCommand            = new RelayCommand(Discard);
    }

    // Navigation properties
    public IAsyncRelayCommand NavigateToSettingsCommand { get; }
    public IAsyncRelayCommand NavigateToDummyCommand { get; }

    // Cancellation properties
    public IRelayCommand SaveCommand    { get; }
    public IRelayCommand DiscardCommand { get; }

    public string DocumentText
    {
        get => _documentText;
        set
        {
            if (SetProperty(ref _documentText, value))
            {
                HasUnsavedChanges = _documentText != _savedText;
            }
        }
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set => SetProperty(ref _hasUnsavedChanges, value);
    }

    // Navigation methods
    private async Task NavigateToSettings()
    {
        var settingsPage = _services.GetRequiredService<SettingsPageView>();
        settingsPage.DataContext = _services.GetRequiredService<SettingsPageViewModel>();
        await _navigation.NavigateToAsync(settingsPage);
    }

    private async Task NavigateToDummy()
    {
        var dummyPage = _services.GetRequiredService<DummyPageView>();
        dummyPage.DataContext = _services.GetRequiredService<DummyPageViewModel>();
        await _navigation.NavigateToAsync(dummyPage);
    }

    // Cancellation methods
    private void Save()
    {
        _savedText = _documentText;
        HasUnsavedChanges = false;
    }

    private void Discard()
    {
        DocumentText = _savedText;
        HasUnsavedChanges = false;
    }

    /// <summary>
    /// Called when user tries to navigate away from this page.
    /// Returns false to cancel navigation if there are unsaved changes and user chooses to keep editing.
    /// </summary>
    public async Task<bool> OnDisappearingAsync()
    {
        if (!HasUnsavedChanges)
            return true;

        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Unsaved Changes";
            dialog.Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "You have unsaved changes in your document.",
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = "Do you want to discard these changes and navigate away?",
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(Colors.Orange)
                    }
                }
            };
            dialog.PrimaryButtonText = "Discard Changes";
            dialog.SecondaryButtonText = "Keep Editing";
            dialog.DefaultButton = DefaultButton.Secondary;
        });

        return result == DialogResult.Primary;
    }

    public Task OnAppearingAsync(object? parameter = null)
    {
        return Task.CompletedTask;
    }
}
