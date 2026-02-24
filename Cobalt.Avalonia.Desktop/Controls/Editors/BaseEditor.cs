using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace Cobalt.Avalonia.Desktop.Controls.Editors;

public class BaseEditor : TextBox
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<BaseEditor, string?>(nameof(Title));

    public static readonly StyledProperty<string?> UnitProperty =
        AvaloniaProperty.Register<BaseEditor, string?>(nameof(Unit));

    public static readonly StyledProperty<object?> LeadingContentProperty =
        AvaloniaProperty.Register<BaseEditor, object?>(nameof(LeadingContent));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<BaseEditor, object?>(nameof(ActionContent));

    public static readonly StyledProperty<bool> HasValidationErrorProperty =
        AvaloniaProperty.Register<BaseEditor, bool>(nameof(HasValidationError));

    public static readonly StyledProperty<string?> ValidationErrorMessageProperty =
        AvaloniaProperty.Register<BaseEditor, string?>(nameof(ValidationErrorMessage));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public object? LeadingContent
    {
        get => GetValue(LeadingContentProperty);
        set => SetValue(LeadingContentProperty, value);
    }

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public bool HasValidationError
    {
        get => GetValue(HasValidationErrorProperty);
        set => SetValue(HasValidationErrorProperty, value);
    }

    public string? ValidationErrorMessage
    {
        get => GetValue(ValidationErrorMessageProperty);
        set => SetValue(ValidationErrorMessageProperty, value);
    }

    private string? _parseError;
    private string? _bindingError;

    protected override Type StyleKeyOverride => typeof(BaseEditor);

    static BaseEditor()
    {
        HasValidationErrorProperty.Changed.AddClassHandler<BaseEditor>((editor, _) =>
        {
            editor.PseudoClasses.Set(":error", editor.HasValidationError);
        });
    }

    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        // Do NOT call base — suppresses Avalonia's default DataValidationErrors adorner.
        if (state is BindingValueType.DataValidationError or BindingValueType.DataValidationErrorWithFallback)
            _bindingError = ExtractErrorMessage(error);
        else
            _bindingError = null;

        RefreshValidationState();
    }

    protected void SetParseError(string? message)
    {
        _parseError = message;
        RefreshValidationState();
    }

    protected void ClearParseError()
    {
        _parseError = null;
        RefreshValidationState();
    }

    private void RefreshValidationState()
    {
        var activeError = _parseError ?? _bindingError;
        HasValidationError = activeError != null;
        ValidationErrorMessage = activeError;
    }

    private static string? ExtractErrorMessage(Exception? error)
    {
        if (error == null) return null;

        if (error is AggregateException agg && agg.InnerExceptions.Count == 1)
            error = agg.InnerExceptions[0];

        return error.InnerException?.Message ?? error.Message;
    }
}
