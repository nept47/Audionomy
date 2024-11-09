namespace Audionomy.ViewModels.Pages
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Interfaces;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using System.Collections.ObjectModel;
    using Wpf.Ui.Controls;

    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;
        private List<VoiceLanguageModel> _allLanguages;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private string _azureSpeechServiceKey = String.Empty;

        [ObservableProperty]
        private string _azureLocation = String.Empty;

        [ObservableProperty]
        private bool _isLanguageSelectionTabEnabled = false;

        [ObservableProperty]
        private ObservableCollection<VoiceLanguageModel> _availableLanguages = [];

        [ObservableProperty]
        private ObservableCollection<VoiceLanguageModel> _activeLanguages = [];

        [ObservableProperty]
        private InfoMessageModel _azureInfoBar = new InfoMessageModel();

        private readonly IApplicationSettingsService _applicationSettingsService;

        public SettingsViewModel(IApplicationSettingsService applicationSettingsService)
        {
            _applicationSettingsService = applicationSettingsService;
        }

        public void OnNavigatedFrom()
        {
            AzureInfoBar = new InfoMessageModel();
        }

        public async void OnNavigatedTo()
        {
            var settings = await _applicationSettingsService.LoadSettingsAsync();
            if (settings.ActiveLanguages.Count == 0)
            {
                AzureInfoBar = new InfoMessageModel("No active languages selected. Please go to the Active Languages tab to choose your preferred languages", InfoBarSeverity.Warning, false);
            }

            AzureSpeechServiceKey = settings.Key;
            AzureLocation = settings.Region;

            IsLanguageSelectionTabEnabled = !string.IsNullOrEmpty(settings.Key) && !string.IsNullOrEmpty(settings.Region);

            _allLanguages = settings.Languages;
            AvailableLanguages = new ObservableCollection<VoiceLanguageModel>(settings.Languages
                .Where(x => settings.ActiveLanguages.All(y => y.Locale != x.Locale))
                .ToList());
            ActiveLanguages = new ObservableCollection<VoiceLanguageModel>(settings.ActiveLanguages);
            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        [RelayCommand]
        private async Task OnSaveAzureCredentials()
        {
            try
            {
                await _applicationSettingsService.SaveAzureCredentialsAsync(AzureSpeechServiceKey, AzureLocation);
                var settings = await _applicationSettingsService.LoadSettingsAsync();
                IsLanguageSelectionTabEnabled = !string.IsNullOrEmpty(settings.Key) && !string.IsNullOrEmpty(settings.Region);
                AvailableLanguages = new ObservableCollection<VoiceLanguageModel>(settings.Languages);

                if (settings.ActiveLanguages.Count == 0)
                {
                    AzureInfoBar = new InfoMessageModel("Azure credentials saved", "Now, please select your active languages in the next tab to start using the application.", InfoBarSeverity.Warning, false);
                }
                else
                {
                    AzureInfoBar = new InfoMessageModel("Credentials saved succesfully.", InfoBarSeverity.Success, false);
                    CloseAzureInfoBar();
                }
            }
            catch (Exception ex)
            {
                AzureInfoBar = new InfoMessageModel(ex.Message, InfoBarSeverity.Error);
            }
        }

        public async Task AddActiveLanguage(VoiceLanguageModel language)
        {
            if (language != null && language.Description != null)
            {
                AzureInfoBar = new InfoMessageModel();
                ActiveLanguages.Add(language);
                AvailableLanguages.Remove(language);
                ActiveLanguages = new ObservableCollection<VoiceLanguageModel>(ActiveLanguages.OrderBy(x => x.Description));
                await _applicationSettingsService.SaveActiveLanguagesAsync(ActiveLanguages.ToList());
            }
        }

        public async Task RemoveActiveLanguage(VoiceLanguageModel language)
        {
            if (language != null && language.Description != null)
            {
                ActiveLanguages.Remove(language);
                AvailableLanguages.Add(language);
                AvailableLanguages = new ObservableCollection<VoiceLanguageModel>(AvailableLanguages.OrderBy(x => x.Description));
                ActiveLanguages = new ObservableCollection<VoiceLanguageModel>(ActiveLanguages.OrderBy(x => x.Description));
                AzureInfoBar = ActiveLanguages.Count == 0
            ? new InfoMessageModel("No active languages selected", "Please go to the Active Languages tab to choose your preferred languages", InfoBarSeverity.Warning, false)
            : new InfoMessageModel();
                await _applicationSettingsService.SaveActiveLanguagesAsync(ActiveLanguages.ToList());
            }
        }

        [RelayCommand]
        private async Task OnClearActiveLanguages()
        {
            ActiveLanguages = new ObservableCollection<VoiceLanguageModel>();
            AvailableLanguages = new ObservableCollection<VoiceLanguageModel>(_allLanguages);
            AzureInfoBar = ActiveLanguages.Count == 0
                ? new InfoMessageModel("No active languages selected", "No active languages selected. Please go to the Active Languages tab to choose your preferred languages", InfoBarSeverity.Warning, false)
                : new InfoMessageModel();
            await _applicationSettingsService.SaveActiveLanguagesAsync(ActiveLanguages.ToList());
        }

        async Task CloseAzureInfoBar()
        {
            await Task.Delay(1000);
            AzureInfoBar = new InfoMessageModel();
        }
    }
}
