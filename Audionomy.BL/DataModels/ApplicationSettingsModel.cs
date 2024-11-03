using System.Net.NetworkInformation;

namespace Audionomy.BL.DataModels
{
    public class ApplicationSettingsModel
    {
        public string Key { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public List<VoiceLanguageModel> Languages { get; set; } = [];
        public List<VoiceLanguageModel> ActiveLanguages { get; set; } = [];

        public ApplicationSettingsModel() { }

        public ApplicationSettingsModel(string key, string region)
        {
            Key = key ?? string.Empty;
            Region = region ?? string.Empty;
        }

        public ApplicationSettingsModel(string key, string region, List<VoiceLanguageModel> languages, List<VoiceLanguageModel> activeLanguages)
        {
            Key = key ?? string.Empty;
            Region = region ?? string.Empty;
            Languages = languages ?? [];
            ActiveLanguages = activeLanguages ?? [];
        }
    }
}
