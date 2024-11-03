namespace Audionomy.BL.Utilities
{
    using Audionomy.BL.DataModels;
    using System.Linq;

    public static class VoiceModelMapperUtility
    {
        public static List<VoiceLanguageModel> MapToVoiceLanguageModels(List<AzureVoiceModel> models)
        {
            if (models == null || models.Count == 0)
            {
                return new List<VoiceLanguageModel>();
            }

            return models
               .GroupBy(x => x.Locale)
               .Select(languageGroup => new VoiceLanguageModel(languageGroup.First().LocaleName, languageGroup.Key, MapToVoiceLanguageStyleModels(languageGroup.ToList())))
               .ToList();
        }

        private static List<VoiceLanguageStyleModel> MapToVoiceLanguageStyleModels(List<AzureVoiceModel> azureVoices)
        {
            if (azureVoices == null || azureVoices.Count == 0)
            {
                return new List<VoiceLanguageStyleModel>();
            }

            return azureVoices
               .Select(azureVoice => new VoiceLanguageStyleModel(azureVoice.DisplayName, azureVoice.ShortName, azureVoice.Gender, azureVoice.StyleList, azureVoice.VoiceType, azureVoice.RolePlayList))
               .ToList();
        }
    }
}
