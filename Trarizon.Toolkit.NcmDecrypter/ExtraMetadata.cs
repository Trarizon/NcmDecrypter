using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Trarizon.Toolkit.NcmDecrypter;
public sealed class ExtraMetadata
{
#nullable disable

    [JsonInclude, JsonPropertyName("musicName")]
    public string MusicName { get; private init; }

    [JsonInclude, JsonPropertyName("artist")]
    [SuppressMessage("CodeQuality", "IDE0051", Justification = "For deserialization only")]
    private object[][] Artists
    {
        get => throw new InvalidOperationException();
        init => _artists = value?.Select(artist => artist[0].ToString()).ToArray() ?? [];
    }
    string[] _artists;
    public string[] Performers => _artists;

    [JsonInclude, JsonPropertyName("album")]
    public string Album { get; private set; }

    [JsonInclude, JsonPropertyName("format")]
    public string Format { get; private set; }

#nullable restore

    public byte[]? Image { get; internal set; }

    public string? Comment { get; set; }
}
