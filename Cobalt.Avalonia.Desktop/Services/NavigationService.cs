using System.Collections.ObjectModel;
using Avalonia.Controls;
using Cobalt.Avalonia.Desktop.Controls.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cobalt.Avalonia.Desktop.Services;

/// <summary>
/// Implementation of the navigation service for managing application navigation state and lifecycle.
/// </summary>
public class NavigationService : ObservableObject, INavigationService
{
    /// <summary>
    /// Semaphore used to synchronize navigation operations and prevent concurrent navigations.
    /// </summary>
    private readonly SemaphoreSlim _navigationLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// Sets up the default <see cref="PageFactory"/> that creates page instances and their ViewModels.
    /// </summary>
    public NavigationService()
    {
        PageFactory = navItem =>
        {
            var page = Activator.CreateInstance(navItem.PageType);
            if (page is not Control ctrl)
                throw new InvalidOperationException($"Failed to create page instance for type {navItem.PageType.FullName}");
            var vm = Activator.CreateInstance(navItem.PageViewModelType);
            ctrl.DataContext = vm;
            return ctrl;
        };
    }

    /// <summary>
    /// Gets the currently displayed page Control.
    /// </summary>
    public Control? CurrentPage
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets or sets the currently selected navigation item.
    /// Changing this property triggers navigation to the selected item.
    /// </summary>
    public NavigationItem? SelectedItem
    {
        get;
        set
        {
            var previousItem = field;
            if (SetProperty(ref field, value))
                _ = TryNavigateToItemAsync(value, previousItem);
        }
    }

    /// <summary>
    /// Gets the main navigation items.
    /// </summary>
    public ObservableCollection<NavigationItem> Items { get; } = [];

    /// <summary>
    /// Gets the footer navigation items.
    /// </summary>
    public ObservableCollection<NavigationItem> FooterItems { get; } = [];

    /// <summary>
    /// Gets or sets the factory function that creates page Control instances from navigation items.
    /// The default factory uses Activator.CreateInstance to create both the page and its ViewModel,
    /// setting the ViewModel as the page's DataContext.
    /// </summary>
    public Func<NavigationItem, Control> PageFactory { get; set; }

    /// <summary>
    /// Navigates to the specified page Control.
    /// </summary>
    /// <param name="page">The page Control to navigate to (with DataContext already set).</param>
    /// <param name="parameter">The parameter passed to the navigation request.</param>
    public async Task NavigateToAsync(Control page, object? parameter = null)
    {
        if (!await _navigationLock.WaitAsync(0))
            return;

        try
        {
            if (CurrentPage is not null)
            {
                var allowed = await InvokeDisappearingAsync(CurrentPage);
                if (!allowed)
                    return;
            }

            CurrentPage = page;

            SelectedItem = FindItemForPage(page);

            await InvokeAppearingAsync(page, parameter);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    /// <summary>
    /// Finds the navigation item that corresponds to the given page View type.
    /// </summary>
    /// <param name="page">The page Control to find an item for.</param>
    /// <returns>The matching <see cref="NavigationItem"/> if found; otherwise, <c>null</c>.</returns>
    private NavigationItem? FindItemForPage(Control page)
    {
        var pageType = page.GetType();

        foreach (var item in Items)
        {
            if (item.PageType == pageType)
                return item;
        }

        foreach (var item in FooterItems)
        {
            if (item.PageType == pageType)
                return item;
        }

        return null;
    }

    /// <summary>
    /// Attempts to navigate to the specified navigation item.
    /// </summary>
    /// <param name="targetItem">The target navigation item.</param>
    /// <param name="previousItem">The previously selected navigation item, to be restored if navigation is cancelled.</param>
    private async Task TryNavigateToItemAsync(NavigationItem? targetItem, NavigationItem? previousItem)
    {
        if (!await _navigationLock.WaitAsync(0))
            return;

        try
        {
            if (CurrentPage is not null)
            {
                var allowed = await InvokeDisappearingAsync(CurrentPage);

                if (!allowed)
                {
                    // Navigation canceled - restore previous selection
                    SelectedItem = previousItem;
                    return;
                }
            }

            CurrentPage = targetItem is not null ? PageFactory(targetItem) : null;

            if (CurrentPage is not null)
                await InvokeAppearingAsync(CurrentPage, null);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    /// <summary>
    /// Invokes the <see cref="INavigationViewModel.OnDisappearingAsync"/> method on the ViewModel if it implements the interface.
    /// </summary>
    /// <param name="page">The current page Control.</param>
    /// <returns><c>true</c> if navigation is allowed; otherwise, <c>false</c>.</returns>
    private static async Task<bool> InvokeDisappearingAsync(Control page)
    {
        try
        {
            if (page.DataContext is INavigationViewModel nav)
                return await nav.OnDisappearingAsync();
            return true;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Invokes the <see cref="INavigationViewModel.OnAppearingAsync"/> method on the ViewModel if it implements the interface.
    /// </summary>
    /// <param name="page">The current page Control.</param>
    /// <param name="parameter">The parameter passed to the navigation request.</param>
    private static async Task InvokeAppearingAsync(Control page, object? parameter)
    {
        try
        {
            if (page.DataContext is INavigationViewModel nav)
                await nav.OnAppearingAsync(parameter);
        }
        catch
        {
            // ignored
        }
    }
}
