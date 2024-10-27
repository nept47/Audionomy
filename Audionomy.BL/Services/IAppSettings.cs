using Audionomy.BL.DataModels;

namespace Audionomy.BL.Services
{
    public interface IAppSettings
    {
        Task<SettingsModel> LoadSettingsAsync();
        Task SaveSettingsAsync(SettingsModel settings);
    }
}