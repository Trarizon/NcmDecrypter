using System.Buffers.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Trarizon.Toolkit.NcmDecrypter.Core;
public static class NcmConverter
{
#pragma warning disable IDE0230
    private static readonly Aes Rc4KeyAes = CreateAes(new byte[] { 0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57 });
    private static readonly Aes MetadataAes = CreateAes(new byte[] { 0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28 });
#pragma warning restore IDE0230

    public static string DecryptWithoutMetadata(string ncmPath, string? outputDir = null)
        => Decrypt(ncmPath, outputDir ?? Path.GetDirectoryName(ncmPath)!, false, out _);

    public static Result Decrypt(string ncmPath, string? outputDir = null)
    {
        var outputPath = Decrypt(ncmPath, outputDir ?? Path.GetDirectoryName(ncmPath)!, true, out var metadata);
        return new(outputPath, metadata);
    }

    private static string Decrypt(string ncmPath, string outputDir, bool containsMetadata, out Metadata metadata)
    {
        using var ncmStream = File.OpenRead(ncmPath);

        // 10 | Magic Header
        ncmStream.Seek(10, SeekOrigin.Begin);

        // 4 + len | Rc4 key
        Span<byte> rc4Key = GetRc4Key(ncmStream);

        // 4 + len | Metadata
        metadata = GetMetadata(ncmStream);

        // 4 + 5 | CRC, Gap
        ncmStream.Seek(9, SeekOrigin.Current);

        // 4 + len | Image
        if (containsMetadata) 
            metadata.Image = GetImage(ncmStream);
        else
            SkipImage(ncmStream);

        using var mp3Stream = GetMp3Data(ncmStream, rc4Key);
        string outputFileName = $"{Path.GetFileNameWithoutExtension(ncmPath)}.{metadata.Format ?? "mp3"}";
        string outputPath = Path.Combine(outputDir, outputFileName);

        CreateMp3(mp3Stream, outputPath);
        return outputPath;
    }


    private static Span<byte> GetRc4Key(Stream ncm)
    {
        int len = ncm.ReadInt();
        Debug.Assert(len == 128);

        Span<byte> buffer = stackalloc byte[len];
        ncm.Read(buffer);

        // Xor 0x64
        buffer.Xor(0x64);

        // Decrypt ECB
        byte[] res = Rc4KeyAes.DecryptEcb(buffer, PaddingMode.PKCS7);

        // Skip 'neteasecloudmusic'
        return res.AsSpan(17);
    }

    // Maybe duplicate, mp3 data contains metadata
    // But image data is required
    private static Metadata GetMetadata(Stream ncm)
    {
        int len = ncm.ReadInt();
        Span<byte> buffer = stackalloc byte[len];
        ncm.Read(buffer);

        // Xor 0x63
        buffer.Xor(0x63);

        // Skip '163 key(Don't modify)'
        Span<byte> result = stackalloc byte[Base64.GetMaxDecodedFromUtf8Length(len - 22)];
        // Decode Base64
        Base64.DecodeFromUtf8(buffer[22..], result, out _, out int size);
        // Decrypt ECB
        result = MetadataAes.DecryptEcb(result[..size], PaddingMode.PKCS7);

        // Skip 'music:'
        string json = Encoding.UTF8.GetString(result[6..]);

        return JsonSerializer.Deserialize<Metadata>(json)!;
    }

    private static byte[] GetImage(Stream ncm)
    {
        int len = ncm.ReadInt();
        byte[] buffer = new byte[len];
        ncm.Read(buffer);
        return buffer;
    }

    private static void SkipImage(Stream ncm)
    {
        int len = ncm.ReadInt();
        ncm.Seek(len, SeekOrigin.Current);
    }

    private static MemoryStream GetMp3Data(Stream ncmStream, ReadOnlySpan<byte> rc4Key)
    {
        const int CacheSize = 0x80 * Rc4.BoxLength; // 0x8000;

        Span<byte> sbox = stackalloc byte[Rc4.BoxLength];
        Rc4.InitializeBox(rc4Key, sbox);

        Span<byte> buffer = stackalloc byte[CacheSize];
        var ms = new MemoryStream();

        int len;
        while ((len = ncmStream.Read(buffer)) > 0) {
            if (len < CacheSize)
                buffer = buffer[..len];
            Rc4.Encrypt(buffer, sbox);
            ms.Write(buffer);
        }

        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    private static void CreateMp3(Stream mp3Data, string filePath)
    {
        using var file = File.Create(filePath);
        mp3Data.CopyTo(file);
    }


    private static int ReadInt(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.Read(buffer);
        return BitConverter.ToInt32(buffer);
    }

    private static void Xor(this Span<byte> bytes, byte value)
    {
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] ^= value;
    }

    private static Aes CreateAes(byte[] key)
    {
        Aes aes = Aes.Create();
        aes.Key = key;
        return aes;
    }

}
