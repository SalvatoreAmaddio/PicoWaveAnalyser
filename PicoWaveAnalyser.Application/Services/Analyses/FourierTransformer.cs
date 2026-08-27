using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace PicoWaveAnalyser.Application.Services.Analyses;

public sealed class FourierTransformer : ITransformer
{
    /// <summary>
    /// Computes the frequency spectrum of the supplied voltage samples.
    /// </summary>
    /// <remarks>
    /// The mean is removed before the transform to reduce the DC component.
    /// A Hann window is then applied to reduce spectral leakage when the
    /// captured signal does not contain an exact integer number of cycles.
    ///
    /// The windowed signal is zero-padded to the next power of two before
    /// performing the FFT. Math.NET does not require a power-of-two input,
    /// but zero-padding provides a more densely sampled representation of
    /// the spectrum, which is useful when estimating the location of a
    /// spectral peak between FFT bins.
    ///
    /// Zero-padding does not increase the underlying frequency resolution
    /// of the captured signal or add information to the recording.
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

            double hannWindow = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (samples.Length - 1)));

            spectrum[i] = new Complex(centredSample * hannWindow, 0);
        }

        Fourier.Forward(spectrum, FourierOptions.Matlab);

        return spectrum;
    }

    /// <summary>
    /// Determines the FFT size required to zero-pad the supplied number
    /// of samples to the next power of two.
    /// </summary>
    /// <remarks>
    /// Math.NET can transform arbitrary input sizes, so the power-of-two
    /// size is not required for correctness. Zero-padding is used here to
    /// obtain more closely spaced FFT samples around the spectral peak,
    /// which assists the subsequent peak interpolation.
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