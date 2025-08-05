using Audionomy.BL.Enumerable;

namespace Audionomy.Models
{
    public class TempSynthesizedFileModel
    {
        public string Text { get; set; } = string.Empty;
        public string Locale { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public AudioFormat Format { get; internal set; }
        public string Voice { get; internal set; } = string.Empty;
    }
}
