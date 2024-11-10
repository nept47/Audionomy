namespace Audionomy.ViewModels.Pages
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Extensions;
    using Audionomy.BL.Interfaces;
    using Audionomy.helpers;
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
        private ApplicationSettingsModel _appSettings;
        private UserSettingsModel _userSettings;
        private CancellationTokenSource _cts;

        [ObservableProperty]
        private bool _requiresConfiguration = false;

        [ObservableProperty]
        private Visibility _openedFolderPathVisibility = Visibility.Hidden;

        [ObservableProperty]
        private string _openedFolderPath = string.Empty;

        [ObservableProperty]
        private string _outputFolderPath = string.Empty;

        [ObservableProperty]
        private string _outputFilePath = string.Empty;

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
        private InfoMessageModel _transcriptionInfoBar = new InfoMessageModel();

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
        }

        public void OnNavigatedFrom() { }

        public async void OnNavigatedTo()
        {
            _appSettings = await _applicationSettingsService.LoadSettingsAsync();

            RequiresConfiguration = _appSettings.RequiresConfiguration();
            if (RequiresConfiguration)
            {
                TranscriptionInfoBar = InformationMessageProvider.GetMissingCredentialsMessage();
                return;
            }
            else if (_appSettings.ActiveLanguages.Count == 0)
            {
                RequiresConfiguration = true;
                TranscriptionInfoBar = InformationMessageProvider.GetNoLanguagesSelectedMessage();
                return;
            }
            else
            {
                TranscriptionInfoBar = new InfoMessageModel();
            }

            ComboBoxLanguages = new ObservableCollection<VoiceLanguageModel>(_appSettings.ActiveLanguages);
            _userSettings = await _userSettingsService.LoadSettingsAsync();

            SelectedLanguageIndex = ComboBoxLanguages.Select((language, index) => new { Language = language, Index = index })
                .FirstOrDefault(x => x.Language.Locale == _userSettings.TranscriptionSettings?.Language?.Locale)?.Index ?? 0;
            GenerateSingleFile = _userSettings.TranscriptionSettings.IsSigleFileExportMode;
        }

        [RelayCommand]
        public void OnOpenFolder()
        {
            TranscriptionInfoBar = new InfoMessageModel();
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
                OpenedFolderPath = string.Empty;
                return;
            }

            NumberOfAudioFiles = _audioFileCountingService.ValidWavFiles(openFolderDialog.FolderNames[0]).ToString("D0");
            OpenedFolderPathVisibility = Visibility.Visible;
            OpenedFolderPath = string.Join("\n", openFolderDialog.FolderNames);
            if (string.IsNullOrEmpty(OutputFolderPath))
            {
                OutputFolderPath = string.Join("\n", openFolderDialog.FolderNames);
            }
        }

        [RelayCommand]
        public void OpenOutputFolder()
        {
            TranscriptionInfoBar = new InfoMessageModel();
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
                OpenedFolderPath = string.Empty;
                return;
            }

            NumberOfAudioFiles = _audioFileCountingService.ValidWavFiles(openFolderDialog.FolderNames[0]).ToString("D0");
            OutputFolderPath = string.Join("\n", openFolderDialog.FolderNames);
        }               
        
        [RelayCommand]
        public void OpenOutputFile()
        {
            TranscriptionInfoBar = new InfoMessageModel();
            Progress = new ProgressViewModel();

            SaveFileDialog saveFileDialog = new()
            {
                FileName = $"Transcription_{SelectedLanguage?.Locale}_{DateTime.Now:yyyyMMddHHmm}.txt",
                DefaultExt = "txt",
                Filter = "Text File(*.txt)|*.txt",
                AddExtension = false,
                InitialDirectory = OutputFolderPath,
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            OutputFilePath = saveFileDialog.FileName;
        }

        [RelayCommand]
        private async Task OnTranscribe()
        {
            try
            {
                TranscriptionInfoBar = new InfoMessageModel();

                await SaveUserSelectionsAsync();

                if (!ValidateUserSelectiona())
                {
                    return;
                }

                var outputdirectory = OutputFolderPath;
                var outputFielName = $"Transcription_{SelectedLanguage?.Locale}_{DateTime.Now:yyyyMMddHHmm}.txt";
                if (GenerateSingleFile)
                {
                    outputdirectory = OutputFilePath.GetFolderName();
                    outputFielName = OutputFilePath.GetFilename();
                }


              


                ShowTranscribe = Visibility.Hidden;
                ShowCancelTranscribe = Visibility.Visible;

                var dir = new DirectoryInfo(OpenedFolderPath);
                var files = dir.GetFiles().ToList();

                _cts = new CancellationTokenSource();

                await _transcribeFilesService.TranscribeAsync(
                    files,
                    new SpeechTranscriptionOptionsModel
                    {
                        Language = SelectedLanguage!.Locale,
                        UseSingleOutputFile = GenerateSingleFile,
                        OutputDirectory = outputdirectory,
                        OutputFileName = outputFielName

                    },
                    CreateTranscriptionProgressHandler(),
                    _cts.Token);

                TranscriptionInfoBar = new InfoMessageModel($"Transcription complete!", "Your file(s) are ready.", InfoBarSeverity.Success);

                var task = CloseSpeechSynthesisInfoBar();
            }
            catch (OperationCanceledException)
            {
                TranscriptionInfoBar = InformationMessageProvider.GetTranscriptionCanceledMessage();
            }
            catch (Exception ex)
            {
                TranscriptionInfoBar = InformationMessageProvider.GetGenericErrorMessage(ex.Message);
            }
            finally
            {
                Progress = new ProgressViewModel();
                ShowTranscribe = Visibility.Visible;
                ShowCancelTranscribe = Visibility.Hidden;
            }
        }

        private Progress<TranscriptionResultModel> CreateTranscriptionProgressHandler()
        {
            return new Progress<TranscriptionResultModel>(result =>
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
        }

        private bool ValidateUserSelectiona()
        {
            if (string.IsNullOrEmpty(OpenedFolderPath))
            {
                TranscriptionInfoBar = InformationMessageProvider.GetFolderNotSelectedMessage();
                return false;
            }

            if (SelectedLanguage == null || string.IsNullOrEmpty(SelectedLanguage?.Locale))
            {
                TranscriptionInfoBar = InformationMessageProvider.GetNoSelectLanguageMessage();
                return false;
            }

            if (GenerateSingleFile && string.IsNullOrEmpty(OutputFilePath))
            {
                TranscriptionInfoBar = InformationMessageProvider.GetNoSelectOutputFileMessage();
                return false;
            }

            if (_audioFileCountingService.ValidWavFiles(OpenedFolderPath) == 0)
            {
                TranscriptionInfoBar = InformationMessageProvider.GetNoWavFilesInFolderMessage();
                return false;
            }

            return true;
        }

        private async Task SaveUserSelectionsAsync()
        {
            _userSettings.TranscriptionSettings.IsSigleFileExportMode = GenerateSingleFile;
            _userSettings.TranscriptionSettings.OpenFolderPath = OpenedFolderPath;
            _userSettings.TranscriptionSettings.Language = SelectedLanguage;

            await _userSettingsService.SaveSettingsAsync(_userSettings);
        }

        [RelayCommand]
        private void OnCancelTranscribe()
        {
            Progress = new ProgressViewModel(0, 0, "Cancelling operation...please wait", string.Empty);
            _cts?.Cancel();
        }

        private async Task CloseSpeechSynthesisInfoBar()
        {
            await Task.Delay(5000);
            TranscriptionInfoBar = new InfoMessageModel();
        }
    }
}
