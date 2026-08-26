using System.Numerics;

namespace PicoWaveAnalyser.Application;

public static class ComplexExtentions
{
    //Rearrange the array according to the reversed binary representation of each index.
    public static void ReorderByBitReversal(this Complex[] values)
    {
        int length = values.Length;

        for (int i = 1, j = 0; i < length; i++)
        {
            int bit = length >> 1;

            while ((j & bit) != 0)
            {
                j ^= bit;
                bit >>= 1;
            }

            j ^= bit;

            if (i < j)
            {
                (values[i], values[j]) = (values[j], values[i]);
            }
        }
    }

    public static void ApplyButterflies(this Complex[] values, int blockSize)
    {
        int halfBlockSize = blockSize / 2;

        double angle = -2.0 * Math.PI / blockSize;

        Complex rotation = new(Math.Cos(angle), Math.Sin(angle));

        for (int blockStart = 0; blockStart < values.Length; blockStart += blockSize)
        {
            Complex factor = Complex.One;

            for (int i = 0; i < halfBlockSize; i++)
            {
                int evenIndex = blockStart + i;
                int oddIndex = evenIndex + halfBlockSize;

                Complex even = values[evenIndex];
                Complex odd = factor * values[oddIndex];

                values[evenIndex] = even + odd;
                values[oddIndex] = even - odd;

                factor *= rotation;
            }
        }
    }

    public static double InterpolatePeakBin(this Complex[] spectrum, int peakBin)
    {
        if (peakBin <= 0 || peakBin >= spectrum.Length - 1)
            return peakBin;

        double leftMagnitude = spectrum[peakBin - 1].Magnitude;
        double peakMagnitude = spectrum[peakBin].Magnitude;
        double rightMagnitude = spectrum[peakBin + 1].Magnitude;

        double denominator =
            leftMagnitude
            - (2.0 * peakMagnitude)
            + rightMagnitude;

        if (Math.Abs(denominator) < double.Epsilon)
            return peakBin;

        double offset =
            0.5 *
            (leftMagnitude - rightMagnitude)
            / denominator;

        return peakBin + offset;
    }

    public static int FindDominantBin(this Complex[] spectrum)
    {
        // Since Real-valued signals have a symmetric spectrum.
        // We only need the positive-frequency half.
        int nyquistBin = spectrum.Length / 2;

        // Start at 1 because bin 0 represents DC (0 Hz).
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