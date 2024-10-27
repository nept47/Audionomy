using Audionomy.BL.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Audionomy.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private string _azureSpeechServiceKey = String.Empty;

        [ObservableProperty]
        private string _azureLocation = String.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _azureAvailableLanguages = [];

        [ObservableProperty]
        private ObservableCollection<string> _azureSelectedLanguages = [];

        [ObservableProperty]
        private bool _isLanguageSelectionPanelVisible = false;

        public IAppSettings _appSettings { get; }

        public SettingsViewModel(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void OnNavigatedFrom() { }

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel().ConfigureAwait(false);
        }

        private async Task InitializeViewModel()
        {
            var settings = await _appSettings.LoadSettingsAsync();
            //CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersion = $"UiDesktopApp1 - {GetAssemblyVersion()}";
            AzureSpeechServiceKey = settings.AzureSpeechServiceKey;
            AzureLocation = settings.AzureSpeechServiceLocation;
            AzureAvailableLanguages = new ObservableCollection<string>(settings.AzureSpeechServiceLanguageSupport);
            AzureSelectedLanguages = new ObservableCollection<string>() { "en-gb", "en-us"};
            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        [RelayCommand]
        private void OnSettingsSave()
        {
            _appSettings.SaveSettingsAsync(new BL.DataModels.SettingsModel()
            {
                AzureSpeechServiceKey = _azureSpeechServiceKey,
                AzureSpeechServiceLocation = _azureLocation,
            }).ConfigureAwait(false);
        }

        [RelayCommand]
        private void OnToggleLanguageVisibility()
        {
            IsLanguageSelectionPanelVisible = !IsLanguageSelectionPanelVisible;
        }
    }
}
