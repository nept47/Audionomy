using Audionomy.BL.DataModels;
using Audionomy.BL.Helpers;
using System.Text.Json;

namespace Audionomy.BL.Services
{
    public class AppSettings : IAppSettings
    {
        private string _settingsFilePath;
        private AesEncryption _aesEncryption;

        public AppSettings()
        {
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _settingsFilePath = Path.Combine(roamingPath, "Audionomy", "Settings.config");
            _aesEncryption = new AesEncryption(Environment.UserName);
        }

        public async Task<SettingsModel> LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new SettingsModel();
                }

                using var reader = new StreamReader(_settingsFilePath);
                var fileContent = await reader.ReadToEndAsync();
                return JsonSerializer.Deserialize<SettingsModel>(_aesEncryption.Decrypt(fileContent)) ?? new SettingsModel();
            }
            catch (Exception ex)
            {
                return new SettingsModel();
            }
        }

        public async Task SaveSettingsAsync(SettingsModel settings)
        {
            string directory = Path.GetDirectoryName(_settingsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var writer = new StreamWriter(_settingsFilePath);
            await writer.WriteLineAsync(_aesEncryption.Encrypt(JsonSerializer.Serialize(settings)));
        }
    }
}