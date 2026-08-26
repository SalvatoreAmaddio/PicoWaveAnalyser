using PicoWaveAnalyser.Domain;
using System.Numerics;

namespace PicoWaveAnalyser.Application.Services.Analyses;

public sealed class FrequencyAnalyser
{
    private readonly ITransformer Transformer;

    public FrequencyAnalyser(ITransformer transformer)
    {
        Transformer = transformer;
    }

    public double FindDominantFrequency(Waveform waveform)
    {
        ArgumentNullException.ThrowIfNull(waveform);

        if (waveform.Times.Length < 2)
            throw new ArgumentException("At least two samples are required.");

        double sampleInterval = CalculateSampleInterval(waveform.Times);

        //How many times the waveform was sampled per second?
        double sampleRate = 1.0 / sampleInterval;

        Complex[] spectrum = Transformer.Transform(waveform.Volts);

        int dominantBin = spectrum.FindDominantBin();

        double interpolatedBin = spectrum.InterpolatePeakBin(dominantBin);

        return interpolatedBin * sampleRate / spectrum.Length;
    }

    /// <summary>
    /// Calculates the average sampling interval, in seconds, from the recorded timestamps.
    /// </summary>
    /// <remarks>
    /// For N samples there are N - 1 intervals between the first and last sample.
    /// The average interval is therefore calculated by dividing the total recording
    /// duration by the number of intervals.
    /// </remarks>
    /// <param name="times">The sample timestamps, in seconds.</param>
    /// <returns>The average time, in seconds, between consecutive samples.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when fewer than two timestamps are provided or when the recording
    /// duration is not greater than zero.
    /// </exception>
    private static double CalculateSampleInterval(double[] times)
    {
        if (times.Length < 2)
            throw new ArgumentException("At least two samples are required.", nameof(times));

        double recordingDuration = times[^1] - times[0];

        if (recordingDuration <= 0)
            throw new ArgumentException("Recording duration must be greater than zero.");

        int timeGaps = times.Length - 1;

        return recordingDuration / timeGaps;
    }
}