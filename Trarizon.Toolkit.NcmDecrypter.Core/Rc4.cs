using System.Diagnostics;

namespace Trarizon.Toolkit.NcmDecrypter.Core;

internal static class Rc4
{
    public const int BoxLength = 0x100;

    public static void InitializeBox(ReadOnlySpan<byte> key, Span<byte> sbox)
    {
        Debug.Assert(sbox.Length == BoxLength);

        for (int i = 0; i < BoxLength; i++)
            sbox[i] = (byte)i;

        for (int i = 0, j = 0; i < sbox.Length; i++) {
            j = (j + sbox[i] + key[i % key.Length]) & 0xff;
            (sbox[i], sbox[j]) = (sbox[j], sbox[i]);
        }
    }

    public static void Encrypt(Span<byte> cipher, ReadOnlySpan<byte> sbox)
    {
        for (int iCipher = 0; iCipher < cipher.Length; iCipher++) {
            int i = (iCipher + 1) & 0xff;
            int j = (i + sbox[i]) & 0xff; // Specialized for ncm

            //  No shuffling

            cipher[iCipher] ^= sbox[(sbox[i] + sbox[j]) & 0xff];
        }
    }
}
