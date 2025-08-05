namespace Audionomy.BL.DataModels;

using Audionomy.BL.Enumerable;

public class SpeechSynhesisOptionsModel
{
    public string LanguageCode { get; set; } = string.Empty;
    public string? LanguageStyle { get; }
    public string OutputFile { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool ExportTranscription { get; set; }
    public AudioFormat Format { get; set; }

    public SpeechSynhesisOptionsModel(string text, string languageCode, string outputFile, bool exportTranscription, AudioFormat format, string? languageStyle = null)
    {
        LanguageCode = languageCode;
        OutputFile = outputFile;
        Text = text;
        ExportTranscription = exportTranscription;
        Format = format;
        LanguageStyle = languageStyle;
    }
}
