using System.Diagnostics;
using System.Security.Cryptography;

namespace Trarizon.Toolkit.NcmDecrypter;
internal static class Crypto
{
    public const int Rc4BoxLength = 0x100;

    public static readonly Aes Rc4KeyAes = CreateAes([0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57]);
    public static readonly Aes MetadataAes = CreateAes([0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28]);

    private static Aes CreateAes(byte[] key)
    {
        Aes aes = Aes.Create();
        aes.Key = key;
        return aes;
    }

    public static void Rc4InitalizeBox(Span<byte> box, ReadOnlySpan<byte> key)
    {
        Debug.Assert(box.Length == Rc4BoxLength);

        for (int i = 0; i < box.Length; i++)
            box[i] = (byte)i;

        for (int i = 0, j = 0; i < box.Length; i++) {
            j = (j + box[i] + key[i % key.Length]) & 0xff;
            (box[i], box[j]) = (box[j], box[i]);
        }
    }

    public static void Rc4Encrypt(Span<byte> ciphers, ReadOnlySpan<byte> box)
    {
        Debug.Assert(box.Length == Rc4BoxLength);

        for (int i = 0; i < ciphers.Length; i++) {
            int j = (i + 1) & 0xff;
            int k = (j + box[j]) & 0xff; // specialized for ncm

            // No shuffling

            ciphers[i] ^= box[(box[j] + box[k]) & 0xff];
        }
    }
}
