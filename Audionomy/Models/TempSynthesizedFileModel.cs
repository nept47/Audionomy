namespace Audionomy.Models
{
    public class TempSynthesizedFileModel
    {
        public string Text { get; set; } = string.Empty;
        public string Locale { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool ConvertToAsteriskFormat { get; internal set; }
    }
}
