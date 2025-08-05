namespace Audionomy.BL.Utilities
{
    using NAudio.Wave;

    public class AudioConverterUtility
    {
        public static async Task ConvertToAsteriskFormatAsync(string inputFilePath, string outputFilePath)
        {
            await Task.Run(() =>
            {
                using (var reader = new AudioFileReader(inputFilePath))
                {
                    var format = new WaveFormat(8000, 16, 1);

                    using (var resampler = new MediaFoundationResampler(reader, format))
                    {
                        resampler.ResamplerQuality = 60;
                        WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                    }
                }
            });
        }

        public static async Task ConvertToAmazonConnectFormatAsync(string inputFilePath, string outputFilePath)
        {
            await Task.Run(() =>
            {
                using (var reader = new AudioFileReader(inputFilePath))
                {
                    var pcmFormat = new WaveFormat(8000, 16, 1);
                    using (var resampler = new MediaFoundationResampler(reader, pcmFormat))
                    {
                        resampler.ResamplerQuality = 60;

                        using (var pcmStream = new MemoryStream())
                        {
                            WaveFileWriter.WriteWavFileToStream(pcmStream, resampler);
                            pcmStream.Position = 0;

                            using (var waveStream = new WaveFileReader(pcmStream))
                            {
                                var muLawFormat = WaveFormat.CreateMuLawFormat(8000, 1);
                                using (var muLawStream = new WaveFormatConversionStream(muLawFormat, waveStream))
                                {
                                    WaveFileWriter.CreateWaveFile(outputFilePath, muLawStream);
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}
