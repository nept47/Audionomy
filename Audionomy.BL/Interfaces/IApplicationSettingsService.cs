﻿using Audionomy.BL.DataModels;

namespace Audionomy.BL.Interfaces
{
    public interface IApplicationSettingsService
    {
        Task<ApplicationSettingsModel> LoadSettingsAsync();
        Task<bool> SaveActiveLanguagesAsync(List<VoiceLanguageModel> activeLanguages);
        Task<bool> SaveAzureCredentialsAsync(string key, string region);
    }
}