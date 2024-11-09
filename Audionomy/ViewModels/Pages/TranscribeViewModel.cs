﻿namespace Audionomy.ViewModels.Pages
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Interfaces;
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
        private CancellationTokenSource _cts;

        [ObservableProperty]
        private bool _requiresConfiguration = false;

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
            _navigationWindow = (_serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow)!;
        }

        public async void OnNavigatedFrom() { }

        public async void OnNavigatedTo()
        {
            _appSettings = await _applicationSettingsService.LoadSettingsAsync();
            RequiresConfiguration = _appSettings.RequiresConfiguration();
            if (RequiresConfiguration)
            {
                TranscriptionInfoBar = new InfoMessageModel("Azure credentials are required", "Please configure them before proceeding.", InfoBarSeverity.Informational, false);
                return;
            }
            else if (_appSettings.ActiveLanguages.Count == 0)
            {
                RequiresConfiguration = true;
                TranscriptionInfoBar = new InfoMessageModel("No active languages selected", "Please go to Settings > Active Languages to choose your preferred languages.", InfoBarSeverity.Informational, false);
                return;
            }
            else
            {
                TranscriptionInfoBar = new InfoMessageModel();
            }



            ComboBoxLanguages = new ObservableCollection<VoiceLanguageModel>(_appSettings.ActiveLanguages);
            _userSettings = await _userSettingsService.LoadSettingsAsync();
            await Task.Delay(3000);
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
                TranscriptionInfoBar = new InfoMessageModel();

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
                    TranscriptionInfoBar = new InfoMessageModel("Folder is not selected.", InfoBarSeverity.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(SelectedLanguage.Locale))
                {
                    TranscriptionInfoBar = new InfoMessageModel("Please select a language.", InfoBarSeverity.Warning);
                    return;
                }

                var dir = new DirectoryInfo(OpenedFolderPath);
                var files = dir.GetFiles().ToList();

                if (_audioFileCountingService.ValidWavFiles(OpenedFolderPath) == 0)
                {
                    TranscriptionInfoBar = new InfoMessageModel("There are not wav files in the selected folder.", InfoBarSeverity.Warning);
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

                await _transcribeFilesService.TranscribeAsync(files, new SpeechTranscriptionOptionsModel { Language = SelectedLanguage.Locale, UseSingleOutputFile = GenerateSingleFile, OutputDirectory = OpenedFolderPath }, progress, _cts.Token);

                TranscriptionInfoBar = new InfoMessageModel($"Transcription complete!", "Your file(s) are ready.", InfoBarSeverity.Success);
                CloseSpeechSynthesisInfoBar();
            }
            catch (OperationCanceledException ex)
            {
                TranscriptionInfoBar = new InfoMessageModel(ex.Message, InfoBarSeverity.Warning);
            }
            catch (Exception ex)
            {
                TranscriptionInfoBar = new InfoMessageModel(ex.Message, InfoBarSeverity.Error);
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

        private async Task CloseSpeechSynthesisInfoBar()
        {
            await Task.Delay(5000);
            TranscriptionInfoBar = new InfoMessageModel();
        }
    }
}
