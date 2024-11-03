using System;

namespace Audionomy.BL.DataModels
{
    [Serializable]
    public class VoiceLanguageModel : IComparable<VoiceLanguageModel>
    {
        public string Description { get; set; } = string.Empty;
        public string Locale { get; set; } = string.Empty;
        public List<VoiceLanguageStyleModel> Voices { get; set; } = [];

        public VoiceLanguageModel() { }

        public VoiceLanguageModel(string localeName, string lacale)
        {
            Description = localeName;
            Locale = lacale;
        }

        public VoiceLanguageModel(string localeName, string lacale, List<VoiceLanguageStyleModel> voices)
        {
            Description = localeName;
            Locale = lacale;
            Voices = voices ?? new List<VoiceLanguageStyleModel>();
        }

        public override string ToString()
        {
            return Description;
        }

        public int CompareTo(VoiceLanguageModel? other)
        {
            if (other == null) return 1;

            // Sort by Age, ascending
            return Description.CompareTo(other.Description);
        }
    }
}
