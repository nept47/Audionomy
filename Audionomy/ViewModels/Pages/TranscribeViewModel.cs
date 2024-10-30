namespace Audionomy.ViewModels.Pages
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Interfaces;
    using Audionomy.BL.Services;
    using Audionomy.Services;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.Win32;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows;
    using Wpf.Ui;
    using Wpf.Ui.Controls;

    public partial class TranscribeViewModel : ObservableObject, INavigationAware
    {
        private readonly IAudioFileCountingService _audioFileCountingService;
        private readonly ISettingsService<SecureSettingsModel> _appSettingsService;
        private readonly ISettingsService<UserSettingsModel> _userSettingService;
        private readonly ITranscribeFilesService _transcribeFilesService;
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationWindow _navigationWindow;
        private SecureSettingsModel _appSettings;
        private UserSettingsModel _userSettings;
        private bool _isInitialized;
        CancellationTokenSource _cts;

        [ObservableProperty]
        private Visibility _openedFolderPathVisibility = Visibility.Hidden;

        [ObservableProperty]
        private string _openedFolderPath = string.Empty;

        [ObservableProperty]
        private string _selectedLanguage = string.Empty;

        [ObservableProperty]
        private bool _generateSingleFile = false;

        [ObservableProperty]
        private string _numberOfAudioFiles = string.Empty;

        [ObservableProperty]
        private ProgressViewModel _progress = new ProgressViewModel();

        [ObservableProperty]
        private ErrorViewModel _error = new ErrorViewModel();

        [ObservableProperty]
        private ObservableCollection<string> _comboBoxLanguages;

        [ObservableProperty]
        private Visibility _showTranscribe = Visibility.Visible;

        [ObservableProperty]
        private Visibility _showCancelTranscribe = Visibility.Hidden;

        public TranscribeViewModel(IAudioFileCountingService audioFileCountingService,
            ISettingsService<SecureSettingsModel> appSettingsService,
            ISettingsService<UserSettingsModel> userSettingsService,
            ITranscribeFilesService transcribeFilesService,
            IServiceProvider serviceProvider)
        {
            _audioFileCountingService = audioFileCountingService;
            _appSettingsService = appSettingsService;
            _userSettingService = userSettingsService;
            _transcribeFilesService = transcribeFilesService;
            _serviceProvider = serviceProvider;
            _navigationWindow = (_serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow)!;
        }

        public async void OnNavigatedFrom() { }

        public async void OnNavigatedTo()
        {
            _appSettings = await _appSettingsService.LoadSettingsAsync();
            
            if (!_isInitialized)
            {
                ComboBoxLanguages = new ObservableCollection<string>(_appSettings.AzureSpeechServiceLanguageSelection);

                _userSettings = await _userSettingService.LoadSettingsAsync();
                SelectedLanguage = _userSettings.LastSelectedLanguage;
                GenerateSingleFile = _userSettings.LastSelectedFileModeIsSingle;

                _isInitialized = true;
            }
        }

        [RelayCommand]
        public void OnOpenFolder()
        {
            Error = new ErrorViewModel();
            Progress = new ProgressViewModel();

            OpenFolderDialog openFolderDialog = new()
            {
                Multiselect = false,
                InitialDirectory = OpenedFolderPath ?? (_userSettings.LastSelectedFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
            };

            if (openFolderDialog.ShowDialog() != true)
            {
                return;
            }

            if (openFolderDialog.FolderNames.Length == 0)
            {
                OpenedFolderPath = null;
                return;
            }

            NumberOfAudioFiles = _audioFileCountingService.ValidWavFiles(openFolderDialog.FolderNames[0]).ToString("D0");
            OpenedFolderPathVisibility = Visibility.Visible;
            OpenedFolderPath = string.Join("\n", openFolderDialog.FolderNames);
        }

        [RelayCommand]
        private async Task OnTranscribe()
        {
            try
            {
                Error = new ErrorViewModel();

                await _userSettingService.SaveSettingsAsync(new UserSettingsModel()
                {
                    LastSelectedFileModeIsSingle = GenerateSingleFile,
                    LastSelectedFolder = OpenedFolderPath,
                    LastSelectedLanguage = SelectedLanguage,
                });

                if (string.IsNullOrEmpty(_appSettings.AzureSpeechServiceKey) || string.IsNullOrEmpty(_appSettings.AzureSpeechServiceLocation))
                {
                    _navigationWindow.Navigate(typeof(Views.Pages.SettingsPage));
                    return;
                }

                if (string.IsNullOrEmpty(OpenedFolderPath))
                {
                    Error = new ErrorViewModel("Folder is not selected.", InfoBarSeverity.Warning);
                    return;
                }

                var dir = new DirectoryInfo(OpenedFolderPath);
                var files = dir.GetFiles().ToList();

                if (_audioFileCountingService.ValidWavFiles(OpenedFolderPath) == 0)
                {
                    Error = new ErrorViewModel("There are not wav files in the selected folder.", InfoBarSeverity.Warning);
                    return;
                }

                ShowTranscribe = Visibility.Hidden;
                ShowCancelTranscribe = Visibility.Visible;
                var progress = new Progress<TranscriptionResult>(result =>
                {
                    if (result.Completed)
                    {
                        Progress = new ProgressViewModel(result.TotalFileCount, result.TranscribedFileCount, $"{result.FilePath}", string.Empty);
                    }
                    else
                    {
                        Progress = new ProgressViewModel(result.TotalFileCount, result.TranscribedFileCount, $"Transcribing... {result.FilePath}", $"{result.TranscribedFileCount}/{result.TotalFileCount}");
                    }
                });


                _cts = new CancellationTokenSource();

                if (GenerateSingleFile)
                {
                    await _transcribeFilesService.TranscribeAndSaveAsync(files, new SpeechTranscriptionExtentOptions { LanguageCode = SelectedLanguage }, progress, _cts.Token);
                }
                else
                {
                    await _transcribeFilesService.TranscribeAndSaveAsync(files, new SpeechTranscriptionBaseOptions { LanguageCode = SelectedLanguage }, progress, _cts.Token);
                }
            }
            catch (OperationCanceledException ex)
            {
                Error = new ErrorViewModel(ex.Message, InfoBarSeverity.Warning);
            }
            catch (Exception ex)
            {
                Error = new ErrorViewModel(ex.Message, InfoBarSeverity.Error);
            }
            finally
            {
                Progress = new ProgressViewModel();
                ShowTranscribe = Visibility.Visible;
                ShowCancelTranscribe = Visibility.Hidden;
            }
        }

        [RelayCommand]
        private void OnCancelTranscribe()
        {
            Progress = new ProgressViewModel(0, 0, "Cancelling operation...please wait", string.Empty);
            _cts?.Cancel();
        }
    }
}
