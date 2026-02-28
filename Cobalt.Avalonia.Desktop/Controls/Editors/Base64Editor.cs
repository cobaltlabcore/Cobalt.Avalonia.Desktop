using Enigma.Cryptography.DataEncoding;

namespace Cobalt.Avalonia.Desktop.Controls.Editors;

public class Base64Editor : ByteArrayEditor
{
    private static readonly Base64Service Base64Service = new();

    protected override string FormatValue(byte[] value)
        => Base64Service.Encode(value);

    protected override bool TryParse(string? text, out byte[] result)
    {
        result = [];
        var success = false;
        
        if (text is null) return true;

        try
        {
            result = Base64Service.Decode(text);
            success = true;
        }
        catch
        {
            //ignored
        }

        return success;
    }
}
