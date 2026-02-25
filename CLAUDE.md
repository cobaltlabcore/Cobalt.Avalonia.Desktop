# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**Cobalt.Avalonia.Desktop** is a reusable Avalonia control library. **CobaltAvaloniaDesktopTester** is the companion WinExe app used to develop and test the controls.

## Build & Run

```bash
dotnet build                                        # Build the solution (both projects)
dotnet run --project CobaltAvaloniaDesktopTester     # Run the tester app
```

No test projects exist yet.

## Architecture

- **Framework**: Avalonia 11.3 with FluentTheme (Dark variant), .NET 10.0, C# 14 (LangVersion 14)
- **MVVM**: CommunityToolkit.Mvvm with compiled bindings (`AvaloniaUseCompiledBindingsByDefault=true`)
- **DI**: `Microsoft.Extensions.DependencyInjection` + `Microsoft.Extensions.Hosting` — `Program.cs` builds an `IHost`, registers services, then starts Avalonia
- **Icons**: PhosphorIconsAvalonia — use `IconService.CreateGeometry(Icon.xxx, IconType.regular)` for navigation item icons

### Project Structure

**Cobalt.Avalonia.Desktop** — Reusable control library (no CommunityToolkit dependency). Contains:
- `Controls/` — TemplatedControls organized in subdirectories: `Navigation/`, `Docking/`, `Ribbon/`, `Editors/`, `CalendarSchedule/`, `Displayer2D/`, plus top-level `ContentDialog`, `Overlay`, `InfoBar`, `SettingsCard`, `SettingsCardExpander`
- `Data/` — `CollectionView` subsystem (filtering, sorting, grouping over `IEnumerable` + `INotifyCollectionChanged`)
- `Services/` — Each service control has a matching service + interface: `NavigationService`, `ContentDialogService`, `OverlayService`, `InfoBarService`, `FileDialogService`, `FolderDialogService`
- `Themes/` — Control templates (AXAML). Composed via `Fluent.axaml` and included in App.axaml as `avares://Cobalt.Avalonia.Desktop/Themes/Fluent.axaml`

**CobaltAvaloniaDesktopTester** — Tester application (WinExe). Contains ViewModels, Views, and app entry point. Depends on CommunityToolkit.Mvvm, Enigma.Cryptography, LiveChartsCore, PhosphorIconsAvalonia. Includes an embedded ASP.NET Core Minimal API server (`ApiHostedService`) on localhost:5100.

**Cobalt.Localization** / **Cobalt.Localization.Avalonia** — Localization library (netstandard2.0) with Avalonia markup extension (`TranslateExtension`).

**Cobalt.Iam** — Identity and Access Management (netstandard2.0): User/Role/Group/Session models, JSON-backed repositories, authentication/authorization services.

### Patterns

- **DI registration**: `ServiceCollectionExtensions.cs` uses C# 14 `extension` blocks on `IServiceCollection` to register services, views (Transient), and ViewModels (Singleton). `App.axaml.cs` resolves from the container and wires service hosts.
- **Naming**: Page ViewModels are `{Name}PageViewModel`, views are `{Name}PageView`. VMs go in `ViewModels/`, views in `Views/`. ViewModels inherit from `ObservableObject` directly (no `ViewModelBase`).
- **Navigation**: `NavigationService` holds `NavigationItem` instances. Selecting an item triggers `PageFactory(NavigationItem)` which creates a page View + ViewModel pair. The default factory uses `Activator.CreateInstance`; the tester overrides it with DI-resolved instances. `ContentControl` bound to `CurrentPage` renders the result.
- **Navigation lifecycle**: ViewModels can implement `INavigationViewModel` with `OnDisappearingAsync()` (return `false` to cancel navigation) and `OnAppearingAsync(object? parameter)`. Navigation is serialized with `SemaphoreSlim(1,1)` — concurrent navigations are dropped.
- **Host pattern**: `ContentDialog`, `Overlay`, and `InfoBar` are all hosted as siblings in MainWindow's root `Panel`. Each is registered with its service via `RegisterHost()` from `App.axaml.cs` `OnFrameworkInitializationCompleted`. ViewModels receive services through constructor injection from `MainWindowViewModel`.
- **TemplatedControls**: Custom controls in Cobalt are `TemplatedControl` (not `UserControl`). C# classes go in `Controls/`, AXAML templates in `Themes/`. When adding a new control, also add its template include to `Themes/Fluent.axaml`. Use `{Binding ..., RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}` for two-way template bindings — `{TemplateBinding}` is one-way only in Avalonia.
- **OnApplyTemplate pattern**: Controls find named template parts via `e.NameScope.Find<T>("PART_Name")` in `OnApplyTemplate`. Always detach old event handlers before attaching new ones, since `OnApplyTemplate` can be called multiple times.
- **Pseudo-classes**: State-based styling uses `PseudoClasses.Set(":stateName", condition)`, `PseudoClasses.Add(":state")`, `PseudoClasses.Remove(":state")` — e.g. `:error`, `:expanded`, `:hasContent`, `:horizontal`, `:vertical`.

### Theme System

Colors are defined in `Cobalt.Avalonia.Desktop/Themes/Colors.axaml` using `ResourceDictionary.ThemeDictionaries` with `x:Key="Dark"` and `x:Key="Light"` variants. Brushes in `Brushes.axaml` reference colors via `{DynamicResource}` for runtime theme switching. Use `{DynamicResource CobaltForegroundSecondaryBrush}` (not `StaticResource`) for theme-reactive styling. Key prefixes: `CobaltBackground`, `CobaltSurface`, `CobaltBorder`, `CobaltForeground`, `CobaltAccent`, `CobaltSuccess`, `CobaltWarning`, `CobaltError`.

### Property & Command conventions

- **Observable properties**: Use C# 13 semi-auto properties with `SetProperty`. Never use `[ObservableProperty]`. Classes must NOT be `partial`.
  ```csharp
  public string Text { get; set => SetProperty(ref field, value); } = "default";
  ```
- **Commands**: Declare as `IRelayCommand` / `IAsyncRelayCommand` properties, initialize in the constructor with `new RelayCommand(Method)` / `new AsyncRelayCommand(AsyncMethod)`. Never use `[RelayCommand]`.
  ```csharp
  public IRelayCommand SaveCommand { get; }
  // in constructor:
  SaveCommand = new RelayCommand(Save);
  ```

### Gotchas

- **Namespace collision**: The `Cobalt.Avalonia.Desktop` namespace causes `Avalonia.Media` (and similar) to resolve incorrectly as `Cobalt.Avalonia.Media`. Use `global::Avalonia.Media` or `using global::Avalonia.Media;` in C# files that need Avalonia sub-namespaces, BUT ONLY WHEN ABSOLUTELY NECESSARY.
- **Compiled bindings require `x:DataType`**: Every UserControl/DataTemplate using `{Binding}` needs an explicit `x:DataType` or the build fails with AVLN2100.
- **Hit testing requires a Background**: A `Border` or `Panel` with no `Background` (null) is invisible to pointer events. Set `Background="Transparent"` on elements that need to receive clicks/hover across their full area.
