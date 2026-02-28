# Cobalt.Avalonia.Desktop

A comprehensive, reusable Avalonia UI control library featuring modern navigation, docking, ribbon, editors, 2D canvas, calendar/schedule, and service-based components for desktop applications.

## Features

- **Navigation System** — Sidebar navigation with header/footer items, horizontal/vertical orientation, and async page lifecycle
- **Docking System** — Tabbed docking panels with drag-and-drop, split containers, and serialisable layout model
- **Ribbon Controls** — Ribbon tabs, groups, buttons, toggle buttons, and drop-down menus
- **Editor Controls** — Typed editors for all .NET numeric types, text, Base64, hexadecimal, and byte arrays with built-in validation
- **2D Canvas (Displayer2D)** — Zoomable/pannable 2D drawing surface with shape primitives, images, text, and pluggable interaction handlers
- **Calendar & Schedule** — Month and week views with drag-to-move, resize, mini-calendar sidebar, and event callbacks
- **Service-Based Components** — ContentDialog, Overlay, InfoBar, File/Folder dialogs via injectable services
- **Settings Controls** — SettingsCard, SettingsCardControl (clickable), and SettingsCardExpander for configuration UIs
- **Dark/Light Theme Support** — Complete theming system with dynamic resource switching
- **Compiled Bindings** — Performance-optimized with compiled bindings by default
- **Host Integration** — Works with `Microsoft.Extensions.Hosting` for clean service lifetime and `IHostedService` support

## Requirements

- .NET 10.0 or later
- Avalonia 11.3+
- CommunityToolkit.Mvvm 8.4+ (for ViewModels in your application)

## Controls

### Navigation

| Control | Description |
|---|---|
| `NavigationControl` | Sidebar navigation host with `Items`, `FooterItems`, `SelectedItem`, and `Vertical`/`Horizontal` orientation |
| `NavigationItemControl` | A single navigation entry with `Header`, `IconData`, `PageType`, and `PageViewModelType` |

### Docking

| Control | Description |
|---|---|
| `DockingControl` | Top-level docking container. Holds `Panes` or builds from a `LayoutRoot` model. Handles drag-and-drop between groups |
| `DockPane` | A single dockable panel with `Header`, `PaneContent`, `CanClose`, `CanMove` |
| `DockTabGroup` | Tabbed container for multiple `DockPane` instances with drag detection and close buttons |
| `DockSplitContainer` | Resizable two-pane splitter with configurable `Orientation` and `GridLength` sizes |

Layout model classes for serialisation: `DockPaneModel`, `DockTabGroupModel`, `DockSplitModel`.

### Ribbon

| Control | Description |
|---|---|
| `RibbonControl` | Top-level ribbon with `Tabs` collection and `SelectedTab`/`SelectedIndex` |
| `RibbonTab` | A ribbon tab page with `Header` and `Groups` collection |
| `RibbonGroup` | A named group of controls within a tab |
| `RibbonButton` | Clickable ribbon button with `Header`, `IconData`, `Command` |
| `RibbonToggleButton` | Stateful toggle button with `IsChecked` |
| `RibbonDropDownButton` | Button that opens a popup with `RibbonMenuItem` items |
| `RibbonMenuItem` | Data item for drop-down menus with `Header`, `IconData`, `Command` |

### Editors

All editors inherit from `BaseEditor` and support `Title`, `Unit`, `ActionContent`, `SelectAllTextOnFocus` (default `true`), `HasValidationError`, and `ValidationErrorMessage`.

**Numeric Editors** (with `Value`, `Minimum`, `Maximum`, `Increment`, `FormatString`):

| Control | Type |
|---|---|
| `IntEditor` | `int` |
| `ShortEditor` | `short` |
| `LongEditor` | `long` |
| `UIntEditor` | `uint` |
| `UShortEditor` | `ushort` |
| `ULongEditor` | `ulong` |
| `SingleEditor` | `float` |
| `DoubleEditor` | `double` |
| `DecimalEditor` | `decimal` |

**Text Editors:**

| Control | Description |
|---|---|
| `TextEditor` | Single-line text input |
| `MultiLineTextEditor` | Multi-line text input |

**Specialized Editors:**

| Control | Description |
|---|---|
| `Base64Editor` | Base64 encoded data editing |
| `HexadecimalEditor` | Hexadecimal value editing |
| `ByteArrayEditor` | Byte array editing |

### Displayer2D (2D Canvas)

| Control / Class | Description |
|---|---|
| `Displayer2DControl` | Zoomable/pannable 2D canvas with `DrawingObjects`, `ZoomFactor`, `PanX`, `PanY` |
| `Displayer2DCanvas` | Inner rendering surface |
| `DrawingObject` | Abstract base for all drawable objects (position, size, rotation, z-index, visibility) |
| `RectangleShape` | Rectangle primitive |
| `CircleShape` | Circle primitive |
| `EllipseShape` | Ellipse primitive |
| `LineShape` | Line between two points |
| `PathShape` | Arbitrary path geometry |
| `TextShape` | Rendered text |
| `ImageShape` | Bitmap image |
| `DrawingObjectGroup` | Groups objects with shared visibility/transform |
| `UserInteraction` | Base class for pluggable input handlers (pan, zoom, selection) |
| `DragInteraction` | Built-in pan/zoom interaction |

### Calendar & Schedule

| Control | Description |
|---|---|
| `CalendarScheduleControl` | Full calendar with month/week views, drag-to-move, edge-resize, mini-calendar sidebar |
| `CalendarScheduleItem` | Data class: `Title`, `Start`, `End`, `Color`, `Description` |

Events: `ItemMoved`, `ItemResized` with `CalendarScheduleItemChangedEventArgs`.

### Dialogs & Overlays

| Control | Description |
|---|---|
| `ContentDialog` | Modal dialog overlay with primary/secondary/close buttons and `ShowAsync()` |
| `InfoBarControl` | In-app notification bar with `Severity` (Info/Success/Warning/Error) |
| `OverlayPresenter` | Full-screen overlay host for arbitrary content |

### Settings

| Control | Description |
|---|---|
| `SettingsCard` | Static settings row with `Header`, `Description`, `IconData`, and content slot |
| `SettingsCardControl` | Clickable settings card with `Command` support |
| `SettingsCardExpander` | Expandable settings card with `IsExpanded` toggle |

## Services

| Interface | Implementation | Description |
|---|---|---|
| `INavigationService` | `NavigationService` | Manages navigation items, selection, and async page lifecycle (`OnAppearingAsync`/`OnDisappearingAsync`) |
| `IContentDialogService` | `ContentDialogService` | Shows modal dialogs via a registered `ContentDialog` host |
| `IInfoBarService` | `InfoBarService` | Shows notification bars via a registered `InfoBarControl` host |
| `IOverlayService` | `OverlayService` | Shows full-screen overlays via a registered `OverlayPresenter` host |
| `IFileDialogService` | `FileDialogService` | Wraps `IStorageProvider` for open/save file dialogs (with extension methods for simplified usage) |
| `IFolderDialogService` | `FolderDialogService` | Wraps `IStorageProvider` for folder picker dialogs (with extension methods for simplified usage) |

Page ViewModels can implement `INavigationViewModel` for lifecycle hooks:
- `OnDisappearingAsync()` — called before navigating away; return `false` to cancel
- `OnAppearingAsync()` — called after the page becomes active

## Installation

### 1. Add Package Reference

```bash
dotnet add package Cobalt.Avalonia.Desktop
```

### 2. Include Theme Resources

In your `App.axaml`, include the Cobalt theme resources:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="YourApp.App">
  <Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://Cobalt.Avalonia.Desktop/Themes/Fluent.axaml" />
  </Application.Styles>
</Application>
```

### 3. Set Up Hosting and Services

Cobalt uses `Microsoft.Extensions.Hosting` for clean service lifetime management. Configure the host in `Program.cs`:

```csharp
using Avalonia;
using Microsoft.Extensions.Hosting;

namespace YourApp;

sealed class Program
{
    internal static IHost? AppHost { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        AppHost = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddHostedServices();   // your IHostedService registrations
                services.AddCobaltServices();   // Cobalt service singletons
                services.AddPagesAndViewModels(); // views (Transient) + view models (Singleton)
            })
            .Build();

        AppHost.Start();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        // Clean shutdown after the window closes
        AppHost.StopAsync().GetAwaiter().GetResult();
        AppHost.Dispose();
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
```

Organise your registrations with extension methods:

```csharp
using Cobalt.Avalonia.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace YourApp;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddHostedServices()
        {
            // Register your IHostedService implementations here
            _ = services.AddHostedService<ApiHostedService>();
        }

        public void AddCobaltServices()
        {
            _ = services.AddSingleton<INavigationService, NavigationService>();
            _ = services.AddSingleton<IContentDialogService, ContentDialogService>();
            _ = services.AddSingleton<IInfoBarService, InfoBarService>();
            _ = services.AddSingleton<IOverlayService, OverlayService>();
            _ = services.AddSingleton<IFileDialogService, FileDialogService>();
            _ = services.AddSingleton<IFolderDialogService, FolderDialogService>();
        }

        public void AddPagesAndViewModels()
        {
            _ = services.AddSingleton<MainWindow>();
            _ = services.AddSingleton<MainWindowViewModel>();

            // Views are Transient so each navigation creates a fresh instance
            _ = services.AddTransient<HomePageView>();
            _ = services.AddTransient<SettingsPageView>();

            // ViewModels are Singleton to preserve state across navigations
            _ = services.AddSingleton<HomePageViewModel>();
            _ = services.AddSingleton<SettingsPageViewModel>();
        }
    }
}
```

### 4. Configure App.axaml.cs

Resolve services from the host and register Cobalt host controls:

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cobalt.Avalonia.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace YourApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = Program.AppHost?.Services ?? BuildDesignerServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = services.GetRequiredService<MainWindow>();
            var vm = services.GetRequiredService<MainWindowViewModel>();
            mainWindow.DataContext = vm;

            // Register service hosts
            services.GetRequiredService<IContentDialogService>().RegisterHost(mainWindow.HostDialog);
            services.GetRequiredService<IOverlayService>().RegisterHost(mainWindow.HostOverlay);
            services.GetRequiredService<IInfoBarService>().RegisterHost(mainWindow.HostInfoBar);
            services.GetRequiredService<IFileDialogService>().SetStorageProvider(mainWindow.StorageProvider);
            services.GetRequiredService<IFolderDialogService>().SetStorageProvider(mainWindow.StorageProvider);

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Fallback for the Avalonia Designer, which bypasses Program.Main().
    /// </summary>
    private static IServiceProvider BuildDesignerServices()
    {
        var collection = new ServiceCollection();
        collection.AddCobaltServices();
        collection.AddPagesAndViewModels();
        return collection.BuildServiceProvider();
    }
}
```

### 5. Set Up Host Controls in MainWindow

For ContentDialog, Overlay, and InfoBar services, add host controls as siblings in your `MainWindow.axaml`:

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="using:Cobalt.Avalonia.Desktop.Controls"
        x:Class="YourApp.MainWindow">

  <Panel>
    <!-- Your main content -->
    <ContentControl Content="{Binding CurrentPage}" />

    <!-- Service host controls (must be siblings at root level) -->
    <controls:ContentDialog x:Name="HostDialog" />
    <controls:OverlayPresenter x:Name="HostOverlay" />
    <controls:InfoBarControl x:Name="HostInfoBar" />
  </Panel>
</Window>
```

## Adding an IHostedService

The hosting setup makes it easy to run background services alongside your Avalonia app. Here is the `ApiHostedService` from the tester application, which spins up a Minimal API web server:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace YourApp;

public class ApiHostedService(IConfiguration configuration) : IHostedService
{
    private WebApplication? _app;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var port = configuration.GetValue("Api:Port", 5100);
        var title = configuration.GetValue("Api:Title", "My App API");

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls($"http://localhost:{port}");

        _app = builder.Build();

        _app.MapGet("/", () => Results.Ok(new { Title = title, Status = "Running" }));
        _app.MapGet("/api/info", () => Results.Ok(new
        {
            Title = title,
            Port = port,
            Environment.MachineName,
            StartedAt = DateTime.UtcNow
        }));

        await _app.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_app is not null)
        {
            await _app.StopAsync(cancellationToken);
            await _app.DisposeAsync();
        }
    }
}
```

Register it in your `AddHostedServices()` method:

```csharp
public void AddHostedServices()
{
    _ = services.AddHostedService<ApiHostedService>();
}
```

Configure port and title via `appsettings.json`:

```json
{
  "Api": {
    "Port": 5100,
    "Title": "My App API"
  }
}
```

The host starts before the Avalonia window opens and shuts down cleanly after it closes, so hosted services run for the entire application lifetime.

## Usage Examples

### Navigation Service

```csharp
public class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        _navigationService.Items.Add(new NavigationItemControl
        {
            Header = "Home",
            IconData = IconService.CreateGeometry(Icon.House, IconType.regular),
            PageType = typeof(HomePageView),
            PageViewModelType = typeof(HomePageViewModel)
        });
    }
}
```

### ContentDialog Service

```csharp
var result = await _dialogService.ShowMessageAsync(
    title: "Confirm Action",
    content: "Are you sure you want to proceed?",
    primaryButtonText: "Yes",
    closeButtonText: "No"
);

if (result == DialogResult.Primary)
{
    // User clicked Yes
}
```

### InfoBar Service

```csharp
await _infoBarService.ShowAsync(bar =>
{
    bar.Title = "Success";
    bar.Message = "Operation completed successfully.";
    bar.Severity = InfoBarSeverity.Success;
});
```

### Overlay Service

```csharp
await _overlayService.ShowAsync(new LoadingView());

// Later...
await _overlayService.HideAsync();
```

### File Dialog Service

```csharp
var files = await _fileDialogService.ShowOpenFileDialogAsync(
    title: "Open File",
    allowMultiple: false,
    filter: new[] { new FilePickerFileType("Text Files") { Patterns = new[] { "*.txt" } } }
);
```

### Folder Dialog Service

```csharp
var folders = await _folderDialogService.ShowOpenFolderDialogAsync(
    title: "Select Folder",
    allowMultiple: false
);
```

### Editor Controls

```xml
<editors:IntEditor Title="Port"
                   Unit="TCP"
                   Value="{Binding Port}"
                   Minimum="1"
                   Maximum="65535"
                   HasValidationError="{Binding HasPortError}"
                   ValidationErrorMessage="{Binding PortErrorMessage}" />
```

## Project Structure

```
Cobalt.Avalonia.Desktop/
├── Controls/
│   ├── Navigation/          # NavigationControl, NavigationItemControl
│   ├── Docking/             # DockingControl, DockPane, DockTabGroup, DockSplitContainer
│   ├── Ribbon/              # RibbonControl, RibbonTab, RibbonGroup, RibbonButton, etc.
│   ├── Editors/             # BaseEditor, typed editors (Int, Double, Text, Base64, etc.)
│   ├── CalendarSchedule/    # CalendarScheduleControl
│   ├── Displayer2D/         # Displayer2DControl, shapes, interaction handlers
│   ├── ContentDialog.cs     # Modal dialog control
│   ├── OverlayPresenter.cs  # Overlay host control
│   ├── InfoBarControl.cs    # Notification bar control
│   ├── SettingsCard.cs      # Static settings row
│   ├── SettingsCardControl.cs   # Clickable settings card
│   └── SettingsCardExpander.cs  # Expandable settings card
├── Services/
│   ├── NavigationService.cs
│   ├── ContentDialogService.cs
│   ├── InfoBarService.cs
│   ├── OverlayService.cs
│   ├── FileDialogService.cs
│   └── FolderDialogService.cs
└── Themes/
    ├── Fluent.axaml          # Main theme include
    ├── Colors.axaml          # Color definitions (Dark/Light variants)
    └── Brushes.axaml         # Brush resources
```

## Key Patterns

- **TemplatedControls** — All custom controls use `TemplatedControl`, not `UserControl`
- **ViewLocator** — Automatic view resolution by replacing "ViewModel" with "View" in type names
- **Service Host Pattern** — Dialog/overlay/infobar services require host controls registered at startup
- **Two-Way Bindings** — Use `{Binding ..., RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}` in templates (`{TemplateBinding}` is one-way only)
- **Theme Resources** — Use `{DynamicResource}` for theme-reactive styling
- **Hosting** — `Microsoft.Extensions.Hosting` manages the app lifecycle; `IHostedService` implementations run alongside the Avalonia window

## Theme System

Access theme colors and brushes using dynamic resources:

```xml
<Border Background="{DynamicResource CobaltSurfacePrimaryBrush}"
        BorderBrush="{DynamicResource CobaltBorderPrimaryBrush}">
```

Available resource prefixes:
- `CobaltBackground*` — Background colors
- `CobaltSurface*` — Surface/card backgrounds
- `CobaltBorder*` — Border colors
- `CobaltForeground*` — Text colors
- `CobaltAccent*` — Accent colors
- `CobaltSuccess*`, `CobaltWarning*`, `CobaltError*` — Semantic colors

## Development

### Build

```bash
dotnet build
```

### Run Tester Application

```bash
dotnet run --project CobaltAvaloniaDesktopTester
```

The tester application (`CobaltAvaloniaDesktopTester`) provides live examples and a testing playground for all controls.
