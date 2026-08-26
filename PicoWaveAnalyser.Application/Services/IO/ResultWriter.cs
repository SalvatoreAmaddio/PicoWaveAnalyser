using PicoWaveAnalyser.Domain;
using System.Globalization;

namespace PicoWaveAnalyser.Application.Services.IO;

public sealed class ResultWriter
{
    public async Task WriteAsync(string path, IEnumerable<FrequencyResult> results, CancellationToken cancellationToken = default)
    {
        await using StreamWriter writer = new StreamWriter(path, false);
        await writer.WriteLineAsync("Filename,FrequencyHz".AsMemory(), cancellationToken);

        foreach (FrequencyResult result in results)
        {
            string line = $"{result.FileName},{result.FrequencyHz.ToString("G17", CultureInfo.InvariantCulture)}";
            await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
        }
    }
}