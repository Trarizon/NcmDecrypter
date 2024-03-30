using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Toolkit.NcmDecrypter;
internal static class Utils
{
    public static int ReadInt32(this Stream stream)
    {
        int result = 0;
        stream.Read(MemoryMarshal.CreateSpan(ref Unsafe.As<int, byte>(ref result), sizeof(byte)));
        return result;
    }


    public static void BitwiseXor(this Span<byte> bytes, byte value)
    {
        foreach (ref byte b in bytes)
            b ^= value;
    }
}
