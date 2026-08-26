using System.Numerics;

namespace PicoWaveAnalyser.Application.Services.Analyses;

public sealed class FourierTransformer : ITransformer
{
    public Complex[] Transform(double[] samples)
    {
        ArgumentNullException.ThrowIfNull(samples);

        if (samples.Length < 2)
            throw new ArgumentException("At least two samples are required.", nameof(samples));

        int fftSize = GetNextPowerOfTwo(samples.Length);

        Complex[] spectrum = new Complex[fftSize];

        double mean = samples.Average();

        for (int i = 0; i < samples.Length; i++)
        {
            double centredSample = samples[i] - mean;

            double window = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (samples.Length - 1)));

            spectrum[i] = new Complex(centredSample * window, 0);
        }

        TransformInPlace(spectrum);

        return spectrum;
    }

    private static void TransformInPlace(Complex[] values)
    {
        int length = values.Length;

        values.ReorderByBitReversal();

        for (int blockSize = 2; blockSize <= length; blockSize *= 2)
        {
            values.ApplyButterflies(blockSize);
        }
    }

    private static int GetNextPowerOfTwo(int value)
    {
        int result = 1;

        while (result < value)
        {
            result *= 2;
        }

        return result;
    }
}