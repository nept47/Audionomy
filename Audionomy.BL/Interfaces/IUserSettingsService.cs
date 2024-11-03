namespace Audionomy.BL.Interfaces
{
    using Audionomy.BL.DataModels;

    public interface IUserSettingsService
    {
        Task<UserSettingsModel> LoadSettingsAsync();

        Task<bool> SaveSettingsAsync(UserSettingsModel settings);
    }
}
