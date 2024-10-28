namespace Audionomy.ViewModels.Pages
{
    using Audionomy.BL.DataModels;
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
        private readonly IAppSettings _appSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationWindow _navigationWindow;
        private SettingsModel _settings;
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
            IAppSettings appSettings,
            IServiceProvider serviceProvider)
        {
            _audioFileCountingService = audioFileCountingService;
            _appSettings = appSettings;
            _serviceProvider = serviceProvider;
            _navigationWindow = (_serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow)!;
        }

        public async void OnNavigatedFrom() { }

        public async void OnNavigatedTo()
        {
            _settings = await _appSettings.LoadSettingsAsync().ConfigureAwait(false);
            ComboBoxLanguages = new ObservableCollection<string>(_settings.AzureSpeechServiceLanguageSelection);
        }

        [RelayCommand]
        public void OnOpenFolder()
        {
            Error = new ErrorViewModel();
            Progress = new ProgressViewModel();

            OpenFolderDialog openFolderDialog = new()
            {
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
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

                if (string.IsNullOrEmpty(_settings.AzureSpeechServiceKey) || string.IsNullOrEmpty(_settings.AzureSpeechServiceLocation))
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

                var transcribeFilesService = new TranscribeFilesService(_settings.AzureSpeechServiceKey, _settings.AzureSpeechServiceLocation);

                _cts = new CancellationTokenSource();

                if (GenerateSingleFile)
                {
                    await transcribeFilesService.TranscribeAndSaveAsync(files, new SpeechTranscriptionExtentOptions { LanguageCode = SelectedLanguage }, progress, _cts.Token);
                }
                else
                {
                    await transcribeFilesService.TranscribeAndSaveAsync(files, new SpeechTranscriptionBaseOptions { LanguageCode = SelectedLanguage }, progress, _cts.Token);
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
        private async Task OnCancelTranscribe()
        {
            Progress = new ProgressViewModel(0, 0, "Cancelling operation...please wait", string.Empty);
            _cts?.Cancel();
        }
    }
}
