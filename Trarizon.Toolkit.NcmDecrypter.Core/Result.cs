namespace Trarizon.Toolkit.NcmDecrypter.Core;
public readonly struct Result
{
    public string OutputPath { get; }
    public Metadata Metadata { get; }

    public Result(string outputPath, Metadata metadata)
    {
        OutputPath = outputPath;
        Metadata = metadata;
    }
}
