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
        private readonly IApplicationSettingsService _applicationSettingsService;
        private readonly IUserSettingsService _userSettingsService;
        private readonly ITranscribeFilesService _transcribeFilesService;
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationWindow _navigationWindow;
        private ApplicationSettingsModel _appSettings;
        private UserSettingsModel _userSettings;
        private bool _isInitialized;
        CancellationTokenSource _cts;

        [ObservableProperty]
        private Visibility _openedFolderPathVisibility = Visibility.Hidden;

        [ObservableProperty]
        private string _openedFolderPath = string.Empty;

        [ObservableProperty]
        private VoiceLanguageModel? _selectedLanguage = new VoiceLanguageModel();

        [ObservableProperty]
        private bool _generateSingleFile = false;

        [ObservableProperty]
        private string _numberOfAudioFiles = string.Empty; 
        
        [ObservableProperty]
        private int _selectedLanguageIndex = 0;

        [ObservableProperty]
        private ProgressViewModel _progress = new ProgressViewModel();

        [ObservableProperty]
        private ErrorViewModel _error = new ErrorViewModel();

        [ObservableProperty]
        private ObservableCollection<VoiceLanguageModel> _comboBoxLanguages;

        [ObservableProperty]
        private Visibility _showTranscribe = Visibility.Visible;

        [ObservableProperty]
        private Visibility _showCancelTranscribe = Visibility.Hidden;

        public TranscribeViewModel(IAudioFileCountingService audioFileCountingService,
            IApplicationSettingsService applicationSettingsService,
            IUserSettingsService userSettingsService,
            ITranscribeFilesService transcribeFilesService,
            IServiceProvider serviceProvider)
        {
            _audioFileCountingService = audioFileCountingService;
            _applicationSettingsService = applicationSettingsService;
            _userSettingsService = userSettingsService;
            _transcribeFilesService = transcribeFilesService;
            _serviceProvider = serviceProvider;
            _navigationWindow = (_serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow)!;
        }

        public async void OnNavigatedFrom() { }

        public async void OnNavigatedTo()
        {
            _appSettings = await _applicationSettingsService.LoadSettingsAsync();
            ComboBoxLanguages = new ObservableCollection<VoiceLanguageModel>(_appSettings.ActiveLanguages);
            _userSettings = await _userSettingsService.LoadSettingsAsync();
            SelectedLanguageIndex = ComboBoxLanguages.Select((language, index) => new { Language = language, Index = index })
                .FirstOrDefault(x => x.Language.Locale == _userSettings.TranscriptionSettings?.Language?.Locale)?.Index ?? 0;

            if (!_isInitialized)
            {

                _userSettings = await _userSettingsService.LoadSettingsAsync();
                SelectedLanguage = _userSettings.TranscriptionSettings.Language;
                GenerateSingleFile = _userSettings.TranscriptionSettings.IsSigleFileExportMode;

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
                InitialDirectory = OpenedFolderPath ?? (_userSettings.TranscriptionSettings.OpenFolderPath ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
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


                _userSettings.TranscriptionSettings.IsSigleFileExportMode = GenerateSingleFile;
                _userSettings.TranscriptionSettings.OpenFolderPath = OpenedFolderPath;
                _userSettings.TranscriptionSettings.Language = SelectedLanguage;

                await _userSettingsService.SaveSettingsAsync(_userSettings);

                if (string.IsNullOrEmpty(_appSettings.Key) || string.IsNullOrEmpty(_appSettings.Region))
                {
                    _navigationWindow.Navigate(typeof(Views.Pages.SettingsPage));
                    return;
                }

                if (string.IsNullOrEmpty(OpenedFolderPath))
                {
                    Error = new ErrorViewModel("Folder is not selected.", InfoBarSeverity.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(SelectedLanguage.Locale))
                {
                    Error = new ErrorViewModel("Please select a language.", InfoBarSeverity.Warning);
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
                var progress = new Progress<TranscriptionResultModel>(result =>
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
                    await _transcribeFilesService.TranscribeAndSaveAsync(files, new SpeechTranscriptionExtentOptionsModel { Locate = SelectedLanguage.Locale }, progress, _cts.Token);
                }
                else
                {
                    await _transcribeFilesService.TranscribeAndSaveAsync(files, new SpeechTranscriptionBaseOptionsModel { Locate = SelectedLanguage.Locale }, progress, _cts.Token);
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
