using PicoWaveAnalyser.Application.Services.Analyses;
using PicoWaveAnalyser.Application.Services.IO;
using System.Numerics;
using PicoWaveAnalyser.Domain;
using Xunit.Abstractions;

namespace PicoWaveAnalyser.Tests;

public class FrequencyAnalyserTests
{
    private readonly ITestOutputHelper _output;

    public FrequencyAnalyserTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void FindDominantFrequency_WhenWaveIs100Hz_ReturnsApproximately100Hz()
    {
        const double expectedFrequency = 100.0;
        const double sampleRate = 10_000.0;
        const int sampleCount = 10_000;

        Waveform waveform = CreateWaveform(expectedFrequency, sampleRate, sampleCount);

        FrequencyAnalyser analyser = new(new FourierTransformer());

        double actualFrequency = analyser.FindDominantFrequency(waveform);

        Assert.InRange(actualFrequency, 99.5, 100.5);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1_000)]
    [InlineData(2_500)]
    public void FindDominantFrequency_ReturnsExpectedFrequency(double expectedFrequency)
    {
        const double sampleRate = 10_000.0;
        const int sampleCount = 10_000;

        Waveform waveform = CreateWaveform(expectedFrequency, sampleRate, sampleCount);

        FrequencyAnalyser analyser = new(new FourierTransformer());

        double actualFrequency = analyser.FindDominantFrequency(waveform);

        Assert.InRange(actualFrequency, expectedFrequency - 1, expectedFrequency + 1);
    }

    [Fact]
    public void FindDominantFrequency_ReturnsFrequencyWithHighestAmplitude()
    {
        const double sampleRate = 10_000.0;
        const int sampleCount = 10_000;

        double[] times = new double[sampleCount];
        double[] volts = new double[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            double time = i / sampleRate;

            times[i] = time;

            double strongWave = 5.0 * Math.Sin(2.0 * Math.PI * 100.0 * time);

            double weakWave = 2.0 * Math.Sin(2.0 * Math.PI * 1_000.0 * time);

            volts[i] = strongWave + weakWave;
        }

        Waveform waveform = new(times, volts);

        FrequencyAnalyser analyser = new(new FourierTransformer());

        double actualFrequency = analyser.FindDominantFrequency(waveform);

        Assert.InRange(actualFrequency, 99.0, 101.0);
    }

    [Theory]
    [InlineData(123.45)]
    [InlineData(987.65)]
    [InlineData(2345.67)]
    public void FindDominantFrequency_WithNonRoundFrequency_ReturnsApproximatelyExpected(double expectedFrequency)
    {
        const double sampleRate = 10_000.0;
        const int sampleCount = 10_000;

        Waveform waveform = CreateWaveform(expectedFrequency, sampleRate, sampleCount);

        FrequencyAnalyser analyser = new(new FourierTransformer());

        double actualFrequency = analyser.FindDominantFrequency(waveform);

        Assert.InRange(actualFrequency, expectedFrequency - 1, expectedFrequency + 1);
    }

    [Theory]
    [InlineData(5_000)]
    [InlineData(8_192)]
    [InlineData(10_000)]
    [InlineData(12_345)]
    [InlineData(16_384)]
    public void FindDominantFrequency_WithDifferentSampleCounts_ReturnsExpectedFrequency(int sampleCount)
    {
        const double expectedFrequency = 123.45;
        const double sampleRate = 10_000.0;

        Waveform waveform = CreateWaveform(expectedFrequency, sampleRate, sampleCount);

        FrequencyAnalyser analyser = new(new FourierTransformer());

        double actualFrequency = analyser.FindDominantFrequency(waveform);

        Assert.InRange(actualFrequency, expectedFrequency - 1, expectedFrequency + 1);
    }

    [Fact]
    public void FindDominantFrequency_WhenWaveformHasDcOffset_ReturnsWaveFrequency()
    {
        const double expectedFrequency = 100.0;
        const double sampleRate = 10_000.0;
        const int sampleCount = 10_000;

        Waveform waveform = CreateWaveform(expectedFrequency, sampleRate, sampleCount, dcOffset: 20.0);

        FrequencyAnalyser analyser = new(new FourierTransformer());

        double actualFrequency = analyser.FindDominantFrequency(waveform);

        Assert.InRange(actualFrequency, 99.0, 101.0);
    }

    [Fact]
    public void FindDominantFrequency_WithNoise_ReturnsExpectedFrequency()
    {
        const double expectedFrequency = 100.0;
        const double sampleRate = 10_000.0;
        const int sampleCount = 10_000;

        Random random = new(42);

        double[] times = new double[sampleCount];
        double[] volts = new double[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            double time = i / sampleRate;

            times[i] = time;

            double signal = Math.Sin(2.0 * Math.PI * expectedFrequency * time);

            double noise = (random.NextDouble() - 0.5) * 0.2;

            volts[i] = signal + noise;
        }

        Waveform waveform = new(times, volts);

        FrequencyAnalyser analyser = new(new FourierTransformer());

        double actualFrequency = analyser.FindDominantFrequency(waveform);

        Assert.InRange(actualFrequency, 99.0, 101.0);
    }

    private static Waveform CreateWaveform(double frequency, double sampleRate, int sampleCount, double amplitude = 1.0, double dcOffset = 0.0)
    {
        double[] times = new double[sampleCount];
        double[] volts = new double[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            double time = i / sampleRate;

            times[i] = time;

            volts[i] = dcOffset + amplitude * Math.Sin(2.0 * Math.PI * frequency * time);
        }

        return new Waveform(times, volts);
    }

    [Fact]
    public void FindDominantFrequency_WhenFrequencyFallsBetweenBins_InterpolatesPeak()
    {
        const double expectedFrequency = 123.45;
        const double sampleRate = 10_000.0;
        const int sampleCount = 10_000;

        Waveform waveform = CreateWaveform(expectedFrequency, sampleRate, sampleCount);

        FrequencyAnalyser analyser = new(new FourierTransformer());

        double actualFrequency = analyser.FindDominantFrequency(waveform);

        Assert.InRange(actualFrequency, 123.35, 123.55);
    }


    // NOTE:
    // The following two tests were created as diagnostic tests while investigating
    // the signal-processing behaviour of some of the supplied waveform files.
    // They are intentionally excluded from the automated test suite because they
    // were used to inspect and compare results rather than assert expected values.

    //[Theory]
    //[InlineData("Wave280.csv")]
    //[InlineData("Wave228.csv")]
    //[InlineData("Wave209.csv")]
    //public async Task CompareFrequencyEstimators(string fileName)
    //{
    //    WaveformReader reader = new();

    //    string path = Path.Combine(@"C:\Users\salva\Downloads\interviewTest", fileName);

    //    Waveform waveform = await reader.ReadAsync(path);

    //    FrequencyAnalyser fftAnalyser = new(new FourierTransformer());

    //    double fftFrequency = fftAnalyser.FindDominantFrequency(waveform);

    //    double periodFrequency = PeriodFrequencyEstimator.Estimate(waveform);

    //    _output.WriteLine($"{fileName,-15} FFT: {fftFrequency,15:F2} Hz | Period: {periodFrequency,15:F2} Hz");
    //}

    //[Theory]
    //[InlineData("wave401")]
    //public async Task ShowStrongestFrequenciesForWave(string waveformName)
    //{
    //    WaveformReader reader = new();

    //    Waveform waveform = await reader.ReadAsync($@"C:\Users\salva\Downloads\interviewTest\{waveformName}.csv");

    //    FourierTransformer transformer = new();

    //    Complex[] spectrum = transformer.Transform(waveform.Volts);

    //    double recordingDuration = waveform.Times[^1] - waveform.Times[0];

    //    double sampleInterval = recordingDuration / (waveform.Times.Length - 1);

    //    double sampleRate = 1.0 / sampleInterval;

    //    var strongestBins = spectrum.Take(spectrum.Length / 2)
    //                                .Select((value, bin) => new
    //                                {
    //                                    Bin = bin,
    //                                    Magnitude = value.Magnitude
    //                                })
    //                                .Where(x => x.Bin > 0)
    //                                .OrderByDescending(x => x.Magnitude)
    //                                .Take(10)
    //                                .Select(x => new
    //                                {
    //                                    x.Bin,
    //                                    Frequency = x.Bin * sampleRate / spectrum.Length,
    //                                    x.Magnitude
    //                                })
    //                                .ToList();

    //    foreach (var result in strongestBins)
    //    {
    //        _output.WriteLine($"Bin: {result.Bin,4} | " + $"Frequency: {result.Frequency,12:F2} Hz | " + $"Magnitude: {result.Magnitude:F2}");
    //    }
    //}
}