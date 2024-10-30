using Audionomy.BL.DataModels;
using Audionomy.BL.Helpers;
using Audionomy.BL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            catch (Exception ex)
            {
                return new UserSettingsModel();
            }
        }

        public UserSettingsModel LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new UserSettingsModel();
                }

                using var reader = new StreamReader(_settingsFilePath);
                var fileContent = reader.ReadToEnd();
                return JsonSerializer.Deserialize<UserSettingsModel>(fileContent) ?? new UserSettingsModel();
            }
            catch (Exception ex)
            {
                return new UserSettingsModel();
            }
        }

        public async Task<bool> SaveSettingsAsync(UserSettingsModel settings)
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
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
