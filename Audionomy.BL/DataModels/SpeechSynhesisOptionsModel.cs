namespace Audionomy.BL.DataModels
{
    public class SpeechSynhesisOptionsModel
    {
        public string LanguageCode { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool ExportTranscription { get; set; }
        public bool ConvertToAsteriskFormat { get; set; }

        public SpeechSynhesisOptionsModel(string text, string languageCode, string outputFile,  bool exportTranscription, bool convertToAsteriskFormat)
        {
            LanguageCode = languageCode;
            OutputFile = outputFile;
            Text = text;
            ExportTranscription = exportTranscription;
            ConvertToAsteriskFormat = convertToAsteriskFormat;
        }
    }
}
