namespace Audionomy.BL.DataModels
{
    public class TranscriptionSettingModel
    {
        public string? OpenFolderPath { get; set; }
        
        public string? LanguageCode { get; set; }

        public bool IsSigleFileExportMode { get; set; }
    }
}
