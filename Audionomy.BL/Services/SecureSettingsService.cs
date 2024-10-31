namespace Audionomy.BL.Services
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Interfaces;
    using Audionomy.BL.Utilities;
    using System.Text.Json;

    public class SecureSettingsService : ISettingsService<SecureSettingsModel>
    {
        private readonly string _settingsFilePath;
        private readonly AesEncryptionUtility _aesEncryption;
        private readonly string _settingsFileName = "app.config";

        public SecureSettingsService()
        {
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _settingsFilePath = Path.Combine(roamingPath, System.AppDomain.CurrentDomain.FriendlyName, _settingsFileName);
            _aesEncryption = new AesEncryptionUtility(Environment.UserName);
        }

        public async Task<SecureSettingsModel> LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new SecureSettingsModel();
                }

                using var reader = new StreamReader(_settingsFilePath);
                var fileContent = await reader.ReadToEndAsync();
                return JsonSerializer.Deserialize<SecureSettingsModel>(_aesEncryption.Decrypt(fileContent)) ?? new SecureSettingsModel();
            }
            catch
            {
                // TODO: Handle exception
                return new SecureSettingsModel();
            }
        }

        public async Task<bool> SaveSettingsAsync(SecureSettingsModel settings)
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if(directory == null)
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
                await writer.WriteLineAsync(_aesEncryption.Encrypt(JsonSerializer.Serialize(settings)));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}