namespace Audionomy.BL.DataModels;

using Audionomy.BL.Enumerable;

public class SpeechSynthesisSettingModel
{
    public string? OpenFolderPath { get; set; }
    public string? SaveFolderPath { get; set; }
    public VoiceLanguageModel? Language { get; set; }
    public bool GenerateTranscriptionFile { get; set; }
    public AudioFormat Format { get; set; }
    public VoiceLanguageStyleModel? Voice { get; set; }
}
