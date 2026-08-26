using PicoWaveAnalyser.Domain;

namespace PicoWaveAnalyser.Tests;

public static class PeriodFrequencyEstimator
{
    public static double Estimate(Waveform waveform)
    {
        ArgumentNullException.ThrowIfNull(waveform);

        double[] times = waveform.Times;
        double[] volts = waveform.Volts;

        double mean = volts.Average();

        List<double> risingCrossings = [];
        List<double> fallingCrossings = [];

        for (int i = 1; i < volts.Length; i++)
        {
            double previous = volts[i - 1] - mean;
            double current = volts[i] - mean;

            if (previous <= 0 && current > 0)
            {
                double crossingTime = InterpolateCrossing(
                    times[i - 1],
                    times[i],
                    previous,
                    current);

                risingCrossings.Add(crossingTime);
            }
            else if (previous >= 0 && current < 0)
            {
                double crossingTime = InterpolateCrossing(
                    times[i - 1],
                    times[i],
                    previous,
                    current);

                fallingCrossings.Add(crossingTime);
            }
        }

        if (risingCrossings.Count >= 2)
            return CalculateFrequency(risingCrossings);

        if (fallingCrossings.Count >= 2)
            return CalculateFrequency(fallingCrossings);

        throw new InvalidOperationException(
            "Not enough repeated crossings to estimate frequency.");
    }

    private static double InterpolateCrossing(
        double previousTime,
        double currentTime,
        double previousValue,
        double currentValue)
    {
        double fraction =
            -previousValue / (currentValue - previousValue);

        return previousTime +
               fraction * (currentTime - previousTime);
    }

    private static double CalculateFrequency(
        List<double> crossings)
    {
        double duration =
            crossings[^1] - crossings[0];

        int periodCount =
            crossings.Count - 1;

        double averagePeriod =
            duration / periodCount;

        return 1.0 / averagePeriod;
    }
}