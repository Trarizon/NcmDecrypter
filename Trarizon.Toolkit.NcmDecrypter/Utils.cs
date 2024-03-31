using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Toolkit.NcmDecrypter;
internal static class Utils
{
    public static int ReadInt32(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        stream.Read(bytes);
        return Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(bytes));
    }


    public static void BitwiseXor(this Span<byte> bytes, byte value)
    {
        foreach (ref byte b in bytes)
            b ^= value;
    }
}
