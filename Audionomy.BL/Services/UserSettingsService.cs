using Audionomy.BL.DataModels;
using Audionomy.BL.Interfaces;
using System.Text.Json;

namespace Audionomy.BL.Services
{
    public class UserSettingsService : ISettingsService<UserSettingsModel>
    {
        private string _settingsFilePath;
        private readonly string _settingsFileName = "userpreferences.json";

        public UserSettingsService()
        {
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _settingsFilePath = Path.Combine(roamingPath, System.AppDomain.CurrentDomain.FriendlyName, _settingsFileName);
        }

        public async Task<UserSettingsModel> LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new UserSettingsModel();
                }

                using var reader = new StreamReader(_settingsFilePath);
                var fileContent = await reader.ReadToEndAsync();
                return JsonSerializer.Deserialize<UserSettingsModel>(fileContent) ?? new UserSettingsModel();
            }
            catch
            {
                // TODO: Handle exception
                return new UserSettingsModel();
            }
        }

        public async Task<bool> SaveSettingsAsync(UserSettingsModel settings)
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (directory == null)
            {
                return false;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                using var writer = new StreamWriter(_settingsFilePath);
                await writer.WriteLineAsync(JsonSerializer.Serialize(settings));
                return true;
            }
            catch
            {
                return false;

            }
        }
    }
}
