using Enigma.Cryptography.DataEncoding;

namespace Cobalt.Avalonia.Desktop.Controls.Editors;

public class HexadecimalEditor : ByteArrayEditor
{
    private static readonly HexService HexService = new();

    protected override string FormatValue(byte[] value)
        => HexService.Encode(value);

    protected override bool TryParse(string? text, out byte[] result)
    {
        result = [];
        var success = false;
        
        if (text is null) return true;

        try
        {
            result = HexService.Decode(text);
            success = true;
        }
        catch
        {
            //ignored
        }

        return success;
    }
}
