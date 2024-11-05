namespace Audionomy.BL.DataModels
{
    public class SpeechSynhesisOptionsModel
    {
        public string LanguageCode { get; set; } = string.Empty;
        public string? LanguageStyle { get; }
        public string OutputFile { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool ExportTranscription { get; set; }
        public bool ConvertToAsteriskFormat { get; set; }

        public SpeechSynhesisOptionsModel(string text, string languageCode, string outputFile, bool exportTranscription, bool convertToAsteriskFormat, string? languageStyle = null)
        {
            LanguageCode = languageCode;
            OutputFile = outputFile;
            Text = text;
            ExportTranscription = exportTranscription;
            ConvertToAsteriskFormat = convertToAsteriskFormat;
            LanguageStyle = languageStyle;
        }
    }
}
