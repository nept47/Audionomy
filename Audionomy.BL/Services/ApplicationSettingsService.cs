namespace Audionomy.BL.Services
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Interfaces;
    using Audionomy.BL.Utilities;
    using System.Net.Http.Headers;
    using System.Text.Json;

    public class ApplicationSettingsService : IApplicationSettingsService
    {
        private readonly string _settingsFilePath;
        private readonly AesEncryptionUtility _aesEncryption;
        private readonly string _settingsFileName = "app.config";

        public ApplicationSettingsService()
        {
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _settingsFilePath = Path.Combine(roamingPath, AppDomain.CurrentDomain.FriendlyName, _settingsFileName);
            _aesEncryption = new AesEncryptionUtility(Environment.UserName);
        }

        public async Task<ApplicationSettingsModel> LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new ApplicationSettingsModel();
                }

                using var reader = new StreamReader(_settingsFilePath);
                var fileContent = await reader.ReadToEndAsync();

                var settings = JsonSerializer.Deserialize<ApplicationSettingsModel>(_aesEncryption.Decrypt(fileContent)) ?? new ApplicationSettingsModel();
                settings.ActiveLanguages = settings.ActiveLanguages.OrderBy(x => x.Description).ToList();
                settings.Languages = settings.Languages.OrderBy(x => x.Description).ToList();

                return settings;
            }
            catch
            {
                // TODO: Handle exception
                return new ApplicationSettingsModel();
            }
        }

        public ApplicationSettingsModel LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new ApplicationSettingsModel();
                }

                using var reader = new StreamReader(_settingsFilePath);
                var fileContent = reader.ReadToEnd();

                var settings = JsonSerializer.Deserialize<ApplicationSettingsModel>(_aesEncryption.Decrypt(fileContent)) ?? new ApplicationSettingsModel();
                settings.ActiveLanguages = settings.ActiveLanguages.OrderBy(x => x.Description).ToList();
                settings.Languages = settings.Languages.OrderBy(x => x.Description).ToList();

                return settings;
            }
            catch
            {
                // TODO: Handle exception
                return new ApplicationSettingsModel();
            }
        }

        public async Task<bool> SaveAzureCredentialsAsync(string key, string region)
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (directory == null)
            {
                throw new Exception("Can't find roaming path.");
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                var settings = await LoadSettingsAsync();


                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new Exception("Azure key can not be empty.");
                }

                if(string.IsNullOrWhiteSpace(region))
                {
                    throw new Exception("Azure region can not be empty.");
                }

                settings.Region = region;
                settings.Key = key;

                var languages = await GetVoicesFromApiAsync(key, region);
                settings.Languages = VoiceModelMapperUtility.MapToVoiceLanguageModels(languages);

                using var writer = new StreamWriter(_settingsFilePath);
                await writer.WriteLineAsync(_aesEncryption.Encrypt(JsonSerializer.Serialize(settings)));
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> SaveActiveLanguagesAsync(List<VoiceLanguageModel> activeLanguages)
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
                var settings = await LoadSettingsAsync();
                settings.ActiveLanguages = activeLanguages;

                using var writer = new StreamWriter(_settingsFilePath);
                await writer.WriteLineAsync(_aesEncryption.Encrypt(JsonSerializer.Serialize(settings)));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<List<AzureVoiceModel>> GetVoicesFromApiAsync(string key, string region)
        {
            try { 
            var result = new List<AzureVoiceModel>();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
                var response = await client.GetAsync($"https://{region}.tts.speech.microsoft.com/cognitiveservices/voices/list");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(jsonResponse))
                    {
                        result = JsonSerializer.Deserialize<List<AzureVoiceModel>>(jsonResponse);
                    }
                }
                else
                {
                    throw new HttpRequestException($"Failed to fetch data. Status Code: {response.StatusCode}");
                }
            }
            return result ?? [];
            }catch(Exception ex)
            {
                throw;
            }
        }
    }
}