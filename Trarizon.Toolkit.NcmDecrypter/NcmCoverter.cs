using System.Buffers.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Trarizon.Toolkit.NcmDecrypter;
[Flags]
public enum ConverterOptions
{
    None = 0,
    IncludeCover = 1,
    ClientRecognizable = 1 << 1,
    All = IncludeCover | ClientRecognizable,
}

public static class NcmCoverter
{
    public static FileStream Decrypt(string ncmPath, ConverterOptions options, out ExtraMetadata metadata, string? outputDir = null)
    {
        using var ncmStream = File.OpenRead(ncmPath);

        // 10 | Header
        ncmStream.Seek(10, SeekOrigin.Begin);

        // 4 + len
        ReadOnlySpan<byte> rc4Key = ReadRc4Key(ncmStream);

        // 4 + len | datas
        // 4 + 5 | CRC, gap
        // 4 + len | image
        metadata = ReadMetadata(ncmStream, options)!;

        // rest
        var result = File.Create(GenerateFileName(ncmPath, outputDir, metadata.Format ?? "mp3"));
        ReadMusicData(ncmStream, result, rc4Key);

        return result;
    }

    private static ReadOnlySpan<byte> ReadRc4Key(FileStream ncm)
    {
        int len = ncm.ReadInt32();

        Span<byte> buffer = stackalloc byte[len];
        ncm.Read(buffer);

        buffer.BitwiseXor(0x64);

        byte[] res = Crypto.Rc4KeyAes.DecryptEcb(buffer, PaddingMode.PKCS7);

        // skip 'neteasecloudmusic'
        return res.AsSpan(17);
    }

    private static ExtraMetadata ReadMetadata(FileStream ncm, ConverterOptions options)
    {
        int len = ncm.ReadInt32();
        Span<byte> client163Key = stackalloc byte[len];
        ncm.Read(client163Key);

        client163Key.BitwiseXor(0x63);

        // skip '163 key(Don't modify)'
        var base64 = client163Key[22..];
        Span<byte> buffer = stackalloc byte[Base64.GetMaxDecodedFromUtf8Length(base64.Length)];

        Base64.DecodeFromUtf8(base64, buffer, out _, out var size);
        buffer = Crypto.MetadataAes.DecryptEcb(buffer[..size], PaddingMode.PKCS7);
        //                                    skip 'music:'
        string json = Encoding.UTF8.GetString(buffer[6..]);
        var metadata = JsonSerializer.Deserialize<ExtraMetadata>(json)!;

        Debug.Assert(metadata is not null);

        // 163 key

        if (options.HasFlag(ConverterOptions.ClientRecognizable)) {
            metadata.Comment = Encoding.ASCII.GetString(client163Key);
        }

        // 4 + 5 | CRC, gap
        ncm.Seek(9, SeekOrigin.Current);

        // 4 + len | Image
        ReadImage(ncm, options.HasFlag(ConverterOptions.IncludeCover) ? metadata : null);

        return metadata;
    }

    /// <param name="target"><see langword="null"/> if image data is unnecessary</param>
    private static void ReadImage(FileStream ncm, ExtraMetadata? target)
    {
        int len = ncm.ReadInt32();
        if (target is null) {
            ncm.Seek(len, SeekOrigin.Current);
            return;
        }
        else {
            byte[] buffer = new byte[len];
            ncm.Read(buffer);
            target.Image = buffer;
        }
    }

    private static void ReadMusicData(FileStream ncm, FileStream targetStream, ReadOnlySpan<byte> rc4Key)
    {
        var position = targetStream.Position;

        const int CacheSize = 0x80 * Crypto.Rc4BoxLength; // 0x8000;

        Span<byte> sbox = stackalloc byte[Crypto.Rc4BoxLength];
        Crypto.Rc4InitalizeBox(sbox, rc4Key);

        Span<byte> buffer = stackalloc byte[CacheSize];

        int len;
        while ((len = ncm.Read(buffer)) is CacheSize) {
            Crypto.Rc4Encrypt(buffer, sbox);
            targetStream.Write(buffer);
        }
        // rest data
        if (len > 0) {
            buffer = buffer[..len];
            Crypto.Rc4Encrypt(buffer, sbox);
            targetStream.Write(buffer);
        }

        targetStream.Position = position;
    }

    private static string GenerateFileName(string inputPath, string? targetDir, string fileFormat)
    {
        ReadOnlySpan<char> dir = targetDir ?? Path.GetDirectoryName(inputPath.AsSpan());
        ReadOnlySpan<char> fileName = Path.GetFileNameWithoutExtension(inputPath.AsSpan());
        var path = $"{dir}{Path.DirectorySeparatorChar}{fileName}.{fileFormat}";
        return path;
    }
}
