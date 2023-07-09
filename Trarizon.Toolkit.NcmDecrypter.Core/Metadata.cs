namespace Trarizon.Toolkit.NcmDecrypter.Core;
public sealed class Metadata
{
    public string? MusicName { get; }

    // For deserialization
    public object[][]? Artists { get; set; }

    public IEnumerable<string> Performers => Artists?.Select(artist => artist[0].ToString()!) ?? Enumerable.Empty<string>();

    public string? Album { get; }

    public string? Format { get; }

    public byte[]? Image { get; internal set; }
}
