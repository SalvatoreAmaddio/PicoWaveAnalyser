using System.Numerics;

namespace PicoWaveAnalyser.Application.Services.Analyses;

public interface ITransformer
{
    Complex[] Transform(double[] samples);
}