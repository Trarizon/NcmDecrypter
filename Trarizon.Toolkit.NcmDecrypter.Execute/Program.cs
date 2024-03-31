using TagLib;
using Trarizon.Toolkit.NcmDecrypter;

if (args.Length > 0) {
    Run(args);
    return;
}

while (true) {
    Console.Write("> ");
    var input = Console.ReadLine();
    if (string.IsNullOrEmpty(input))
        continue;

    var inputs = SplitArgs(input);
    Run(inputs);
}


static void Run(IReadOnlyList<string> args)
{
    switch (args.Count) {
        case 0: return;
        case 1: RunInternal(args[0], true); return;
        default:
            List<Task> tasks = new(args.Count);
            bool containsMetadata = true;

            if (args.Contains("-nc"))
                containsMetadata = false;

            foreach (var arg in args) {
                if (arg == "-nc")
                    continue;
                else
                    tasks.Add(Task.Run(() => RunInternal(arg, containsMetadata)));
            }

            Task.WhenAll(tasks).Wait();
            return;
    }

    static void RunInternal(string ncmPath, bool containsMetadata)
    {
        try {
            var file = NcmCoverter.Decrypt(ncmPath, containsMetadata ? ConverterOptions.All : ConverterOptions.None, out var metadata);
            var fileName = file.Name;
            file.Close();

            SetMetadata(fileName, metadata);
        } catch (Exception ex) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error:   {Path.GetFileName(ncmPath)}: {ex.Message}");
#if DEBUG
            Console.WriteLine(ex.StackTrace);
#endif
            Console.ForegroundColor = ConsoleColor.Gray;
            return;
        }
        Console.WriteLine($"Success: {Path.GetFileName(ncmPath)} converted.");
    }
}

static void SetMetadata(string fileName, ExtraMetadata metadata)
{
    using var tagFile = TagLib.File.Create(fileName);
    var tag = tagFile.Tag;

    tag.Title = metadata.MusicName;
    tag.Performers = metadata.Performers;
    tag.Album = metadata.Album;
    if (metadata.Image is not null)
        tag.Pictures = [new Picture(new ByteVector(metadata.Image))];
    if (metadata.Comment is not null)
        tag.Comment = metadata.Comment;
    tagFile.Save();
}

static List<string> SplitArgs(string input)
{
    int start = 0;
    List<string> splits = [];

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
        return buffer[..count].ToString();
    }
}
