using Audionomy.BL.DataModels;

namespace Audionomy.BL.Interfaces
{
    public interface ISettingsService<T>
    {
        Task<T> LoadSettingsAsync();

        Task<bool> SaveSettingsAsync(T settings);
    }
}
