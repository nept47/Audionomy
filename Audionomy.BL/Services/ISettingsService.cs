namespace Audionomy.BL.Services
{
    public interface ISettingsService<T>
    {      
        Task<T> LoadSettingsAsync();

        Task<bool> SaveSettingsAsync(T settings);
    }
}
