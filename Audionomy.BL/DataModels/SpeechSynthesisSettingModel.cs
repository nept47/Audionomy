namespace Audionomy.BL.DataModels
{
    public class SpeechSynthesisSettingModel
    {
        public string? OpenFolderPath { get; set; }
        public string? SaveFolderPath { get; set; }
        public VoiceLanguageModel? Language { get; set; }
        public bool GenerateTranscriptionFile { get; set; }
    }
}
