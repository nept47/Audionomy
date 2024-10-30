namespace Audionomy.BL.Services
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Helpers;
    using Audionomy.BL.Interfaces;
    using System.Text.Json;

    public class SecureSettingsService : ISettingsService<SecureSettingsModel>
    {
        private string _settingsFilePath;
        private AesEncryption _aesEncryption;
        private readonly string _settingsFileName = "app.config";

        public SecureSettingsService()
        {
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _settingsFilePath = Path.Combine(roamingPath, System.AppDomain.CurrentDomain.FriendlyName, _settingsFileName);
            _aesEncryption = new AesEncryption(Environment.UserName);
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
            catch (Exception ex)
            {
                return new SecureSettingsModel();
            }
        }

        public SecureSettingsModel LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new SecureSettingsModel();
                }

                using var reader = new StreamReader(_settingsFilePath);
                var fileContent = reader.ReadToEnd();
                return JsonSerializer.Deserialize<SecureSettingsModel>(_aesEncryption.Decrypt(fileContent)) ?? new SecureSettingsModel();
            }
            catch (Exception ex)
            {
                return new SecureSettingsModel();
            }
        }

        public async Task<bool> SaveSettingsAsync(SecureSettingsModel settings)
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
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