using PicoWaveAnalyser.Domain;
using System.Numerics;

namespace PicoWaveAnalyser.Application.Services.Analyses;

public sealed class FrequencyAnalyser
{
    private readonly ITransformer _fourierTransform;

    public FrequencyAnalyser(ITransformer fourierTransform)
    {
        _fourierTransform = fourierTransform;
    }

    public double FindDominantFrequency(Waveform waveform)
    {
        ArgumentNullException.ThrowIfNull(waveform);

        if (waveform.Times.Length != waveform.Volts.Length)
            throw new ArgumentException(
                "Time and voltage arrays must have the same length.");

        if (waveform.Times.Length < 2)
            throw new ArgumentException(
              "At least two samples are required.");

        double sampleInterval = CalculateSampleInterval(waveform.Times);

        //How many times the waveform was sampled per second?
        double sampleRate = 1.0 / sampleInterval;

        Complex[] spectrum = _fourierTransform.Transform(waveform.Volts);

        int dominantBin = spectrum.FindDominantBin();

        double interpolatedBin = spectrum.InterpolatePeakBin(dominantBin);

        double recordingDuration = waveform.Times[^1] - waveform.Times[0];

        double dominantBinFrequency = dominantBin * sampleRate / spectrum.Length;

        double estimatedCycles = dominantBinFrequency * recordingDuration;

        return interpolatedBin * sampleRate / spectrum.Length;
    }

    /// <summary>
    /// Calculates the average time interval, in seconds, between consecutive samples.
    /// </summary>
    /// <param name="times"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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