namespace Audionomy.BL.DataModels
{
    public class UserSettingsModel
    {
        public string LastSelectedFolder { get; set; } = string.Empty;

        public string LastSelectedLanguage { get; set; } = string.Empty;

        public bool LastSelectedFileModeIsSingle { get; set; }
    }
}
