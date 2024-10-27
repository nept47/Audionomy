using System.IO;

namespace Audionomy.Services
{
    class AudioFileCountingService : IAudioFileCountingService
    {
        public int ValidWavFiles(string path)
        {
            return Directory.GetFiles(path).Count(x => Path.GetExtension(x).ToLower() == ".wav");
        }
    }
}
