namespace Cobalt.Avalonia.Desktop.Controls.Editors;

public class MultiLineTextEditor : BaseEditor
{
    protected override Type StyleKeyOverride => typeof(MultiLineTextEditor);

    public MultiLineTextEditor()
    {
        AcceptsReturn = true;
        SelectAllTextOnFocus = false;
        TextWrapping = global::Avalonia.Media.TextWrapping.Wrap;
    }
}
