using Trarizon.Toolkit.NcmDecrypter;
using Trarizon.Toolkit.NcmDecrypter.Core;

if (DoArgs(args))
    return;

while (true) {
    Console.Write("> ");
    var input = Console.ReadLine();
    if (string.IsNullOrEmpty(input))
        continue;

    var inputs = input.SplitAsArguments();
    DoArgs(inputs);
}

static bool DoArgs(IReadOnlyList<string> args)
{
    switch (args.Count) {
        case 0: return false;
        case 1: Do(args[0], true); return true;
        default:
            List<Task> tasks = new(args.Count);
            bool containsMetadata = true;
            bool executed = false;
            foreach (var arg in args) {
                if (arg == "-l")
                    containsMetadata = false;
                else {
                    tasks.Add(Task.Run(() => Do(arg, !containsMetadata)));
                    executed = true;
                }
            }
            Task.WhenAll(tasks).Wait();
            return executed;
    }
}

static void Do(string ncmPath, bool containsMetadata)
{
    try {
        var res = NcmConverter.Decrypt(ncmPath);
        if (containsMetadata) res.SetMetadata();
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