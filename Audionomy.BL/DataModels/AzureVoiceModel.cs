namespace Audionomy.BL.DataModels
{
    public class AzureVoiceModel
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string LocalName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Locale { get; set; } = string.Empty;
        public string LocaleName { get; set; } = string.Empty;
        public List<string> StyleList { get; set; } = [];
        public List<string> SecondaryLocaleList { get; set; } = [];
        public string SampleRateHertz { get; set; } = string.Empty;
        public string VoiceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Dictionary<string, string> ExtendedPropertyMap { get; set; } = [];
        public string WordsPerMinute { get; set; } = string.Empty;
        public List<string> RolePlayList { get; set; } = [];
    }
}
