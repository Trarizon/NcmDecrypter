using TagLib;
using Trarizon.Toolkit.NcmDecrypter.Core;

namespace Trarizon.Toolkit.NcmDecrypter;
internal static class Utility
{
    public static void SetMetadata(this in Result result)
    {
        using var file = TagLib.File.Create(result.OutputPath);
        var tag = file.Tag;

        tag.Title = result.Metadata.MusicName;
        tag.Performers = result.Metadata.Performers?.ToArray();
        tag.Album = result.Metadata.Album;
        tag.Pictures = new[] { new Picture(new ByteVector(result.Metadata.Image)) };

        file.Save();
    }

    public static List<string> SplitAsArguments(this string input)
    {
        int start = 0;
        List<string> splits = new();

        while (start < input.Length) {
            if (char.IsWhiteSpace(input[start])) {
                start++;
                continue;
            }
            int end = start + 1;
            if (input[start] == '\"') {
                start++;
                while (end < input.Length) {
                    if (input[end] == '\"') {
                        if (end + 1 < input.Length && input[end + 1] == '\"') // escape
                            end++;
                        else // End
                            break;
                    }
                    end++;
                }

                splits.Add(Unescape(input.AsSpan(start, end - start)));
            }
            else {
                while (end < input.Length && !char.IsWhiteSpace(input[end]))
                    end++;

                splits.Add(input[start..end]);
            }
            start = end + 1;
        }
        return splits;

        static string Unescape(ReadOnlySpan<char> input)
        {
            Span<char> buffer = stackalloc char[input.Length];
            int count = 0;
            for (int i = 0; i < input.Length; i++) {
                if (input[i] == '"' && input[i + 1] == '"') {
                    count++;
                }
                buffer[count++] = input[i];
            }
            return new string(buffer.Slice(0, count));
        }
    }
}
