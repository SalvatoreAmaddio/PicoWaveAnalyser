# PicoWaveAnalyser

## Note on Short Captures

While testing the supplied files, I noticed that some recordings contain
many cycles of the waveform, while a small number contain only a few.

This makes the exact frequency harder to estimate because a shorter
recording provides less information and produces wider spacing between
FFT frequency bins.

I investigated alternative approaches, including estimating the waveform
period directly. However, these methods answer a slightly different
question: they estimate the repetition period of the signal rather than
directly identifying its highest-amplitude frequency component.

For that reason I kept the FFT approach consistent across all files, as it
most directly matches my interpretation of the requirement.

Zero-padding and peak interpolation improve the estimate between FFT bins,
but cannot recover information that was not present in the original
recording.