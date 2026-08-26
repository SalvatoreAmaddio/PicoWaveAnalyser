using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace PicoWaveAnalyser.Application.Services.Analyses;

public sealed class FourierTransformer : ITransformer
{
    /// <summary>
    /// Performs a Fourier transform on the supplied voltage samples.
    /// </summary>
    /// <remarks>
    /// The mean is removed to eliminate the DC offset and a Hann window is
    /// applied to reduce spectral leakage.
    ///
    /// The signal is zero-padded to the next power of two before the FFT.
    /// Math.NET does not require a power-of-two input size, but the additional
    /// FFT points provide a more densely sampled spectrum for subsequent
    /// peak interpolation.
    /// </remarks>
    public Complex[] Transform(double[] samples)
    {
        ArgumentNullException.ThrowIfNull(samples);

        if (samples.Length < 2)
            throw new ArgumentException("At least two samples are required.", nameof(samples));

        int fftSize = GetZeroPaddedFftSize(samples.Length);

        Complex[] spectrum = new Complex[fftSize];

        double mean = samples.Average();

        for (int i = 0; i < samples.Length; i++)
        {
            double centredSample = samples[i] - mean;

            //Hann window
            double window = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (samples.Length - 1)));

            spectrum[i] = new Complex(centredSample * window, 0);
        }

        Fourier.Forward(spectrum, FourierOptions.Matlab);

        return spectrum;
    }

    /// <summary>
    /// Returns the FFT size used to zero-pad the signal to the next power of two.
    /// </summary>
    /// <remarks>
    /// Math.NET does not require a power-of-two FFT size. The additional zero
    /// samples provide a more densely sampled frequency spectrum, which helps
    /// when locating and interpolating the dominant spectral peak.
    /// </remarks>
    private static int GetZeroPaddedFftSize(int sampleCount)
    {
        int fftSize = 1;

        while (fftSize < sampleCount)
        {
            fftSize *= 2;
        }

        return fftSize;
    }
}