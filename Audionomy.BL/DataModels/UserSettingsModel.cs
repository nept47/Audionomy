namespace Audionomy.BL.DataModels
{
    public class UserSettingsModel
    {
        public TranscriptionSettingModel TranscriptionSettings { get; set; } = new TranscriptionSettingModel();
        public SpeechSynthesisSettingModel SpeechSynthesisSettings { get; set; } = new SpeechSynthesisSettingModel();
    }
}