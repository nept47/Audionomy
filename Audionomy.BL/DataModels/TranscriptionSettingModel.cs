namespace Audionomy.BL.DataModels
{
    public class TranscriptionSettingModel
    {
        public string? OpenFolderPath { get; set; }
        public VoiceLanguageModel? Language { get; set; }
        public bool IsSigleFileExportMode { get; set; }
    }
}
