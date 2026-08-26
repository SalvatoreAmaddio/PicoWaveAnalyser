using System.Numerics;

namespace PicoWaveAnalyser.Application;

public static class ComplexExtensions
{
    public static double InterpolatePeakBin(this Complex[] spectrum, int peakBin)
    {
        if (peakBin <= 0 || peakBin >= spectrum.Length - 1)
            return peakBin;

        double leftMagnitude = spectrum[peakBin - 1].Magnitude;
        double peakMagnitude = spectrum[peakBin].Magnitude;
        double rightMagnitude = spectrum[peakBin + 1].Magnitude;

        double denominator = leftMagnitude - (2.0 * peakMagnitude) + rightMagnitude;

        if (Math.Abs(denominator) < double.Epsilon)
            return peakBin;

        double offset = 0.5 * (leftMagnitude - rightMagnitude) / denominator;

        return peakBin + offset;
    }

    /// <summary>
    /// Finds the FFT bin with the highest magnitude in the positive-frequency
    /// part of the spectrum.
    /// </summary>
    /// <remarks>
    /// For a real-valued input signal, the FFT spectrum is symmetric, so only
    /// the first half needs to be searched.
    ///
    /// Bin 0 is skipped because it represents the DC component (0 Hz), rather
    /// than a frequency component of the waveform.
    /// </remarks>
    /// <param name="spectrum">The frequency spectrum produced by the FFT.</param>
    /// <returns>The index of the bin with the highest magnitude.</returns>
    public static int FindDominantBin(this Complex[] spectrum)
    {
        int nyquistBin = spectrum.Length / 2;

        int dominantBin = 1;
        double highestMagnitude = spectrum[1].Magnitude;

        for (int bin = 2; bin < nyquistBin; bin++)
        {
            double magnitude = spectrum[bin].Magnitude;

            if (magnitude <= highestMagnitude)
                continue;

            highestMagnitude = magnitude;
            dominantBin = bin;
        }

        return dominantBin;
    }
}