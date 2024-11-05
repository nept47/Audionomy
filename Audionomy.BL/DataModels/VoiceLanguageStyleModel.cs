namespace Audionomy.BL.DataModels
{
    public class VoiceLanguageStyleModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public List<string> StyleList { get; set; } = [];
        public string VoiceType { get; set; } = string.Empty;
        public List<string> RolePlayList { get; set; } = [];

        public VoiceLanguageStyleModel() { }

        public VoiceLanguageStyleModel(string displayName, string shortName, string gender, List<string> styleList, string voiceType, List<string> rolePlayList)
        {
            DisplayName = displayName;
            ShortName = shortName;
            Gender = gender;
            StyleList = styleList ?? new List<string>();
            VoiceType = voiceType;
            RolePlayList = rolePlayList ?? new List<string>();
        }

        public override string ToString()
        {
            return $"{DisplayName} ({Gender})";
        }
    }
}
