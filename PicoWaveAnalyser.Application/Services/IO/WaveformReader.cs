using PicoWaveAnalyser.Domain;
using System.Globalization;

namespace PicoWaveAnalyser.Application.Services.IO;

public sealed class WaveformReader
{
    public async Task<Waveform> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        List<double> times = new List<double>();
        List<double> volts = new List<double>();

        using StreamReader reader = new StreamReader(path);

        _ = await reader.ReadLineAsync(cancellationToken);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] columns = line.Split(',');

            if (columns.Length < 2 ||
                !double.TryParse(columns[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double time) ||
                !double.TryParse(columns[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double voltage))
            {
                throw new InvalidDataException($"Invalid waveform row in '{Path.GetFileName(path)}': {line}");
            }

            times.Add(time);
            volts.Add(voltage);
        }

        if (times.Count < 3)
            throw new InvalidDataException($"'{Path.GetFileName(path)}' does not contain enough samples.");

        return new Waveform(times.ToArray(), volts.ToArray());
    }
}