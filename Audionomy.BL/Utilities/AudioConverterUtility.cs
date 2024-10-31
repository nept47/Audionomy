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
    }
}
