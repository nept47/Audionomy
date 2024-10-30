using Audionomy.BL.DataModels;

namespace Audionomy.BL.Interfaces
{
    public interface ISettingsService<T>
    {
        T LoadSettings();

        Task<T> LoadSettingsAsync();

        Task<bool> SaveSettingsAsync(T settings);
    }
}
