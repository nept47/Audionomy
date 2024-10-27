using Audionomy.BL;
using Audionomy.BL.DataModels;
using Audionomy.BL.Services;
using Audionomy.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls;

namespace Audionomy.ViewModels.Pages
{
    public partial class TranscribeViewModel : ObservableObject, INavigationAware
    {
        private readonly IAudioFileCountingService _audioFileCountingService;
        private readonly IAppSettings _appSettings;

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
        private Visibility _progressVisible = Visibility.Hidden;

        [ObservableProperty]
        private int _progressBarMaxValue = 0;

        [ObservableProperty]
        private int _progressBarValue = 0;

        [ObservableProperty]
        private string _progessFileName = string.Empty;

        [ObservableProperty]
        private string _progessSum = string.Empty;

        [ObservableProperty]
        private bool _errorMessageIsOpen = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private InfoBarSeverity _errorSeverity = InfoBarSeverity.Informational;

        [ObservableProperty]
        private ObservableCollection<string> _comboBoxLanguages =
            [
                 "ar-SA","cs-CZ","de-DE","el-GR","en-GB","es-ES","fr-FR","hu-HU","it-IT","nl-NL","pl-PL","pt-PT","ru-RU","sk-SK"
            ];

        public TranscribeViewModel(IAudioFileCountingService audioFileCountingService, IAppSettings appSettings)
        {
            _audioFileCountingService = audioFileCountingService;
            _appSettings = appSettings;
        }

        public void OnNavigatedFrom()
        {
            //throw new NotImplementedException();
        }

        public void OnNavigatedTo()
        {
            // throw new NotImplementedException();
        }

        [RelayCommand]
        public void OnOpenFolder()
        {
            ProgressVisible = Visibility.Collapsed;
            ErrorMessageIsOpen = false;
            ErrorMessage = string.Empty;

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
                var settings = await _appSettings.LoadSettingsAsync();
                if (string.IsNullOrEmpty(OpenedFolderPath))
                {
                    ErrorMessageIsOpen = true;
                    ErrorMessage = "Folder is not selected";
                    ErrorSeverity = InfoBarSeverity.Warning;
                    return;
                }

                var dir = new DirectoryInfo(OpenedFolderPath);
                var files = dir.GetFiles().ToList();

                if (_audioFileCountingService.ValidWavFiles(OpenedFolderPath) == 0)
                {
                    ErrorMessageIsOpen = true;
                    ErrorMessage = "There are not wav files in the selected folder";
                    ErrorSeverity = InfoBarSeverity.Warning;
                    return;
                }


                var transcribeFilesService = new TranscribeFilesService(settings.AzureSpeechServiceKey, settings.AzureSpeechServiceLocation);
                ProgressVisible = Visibility.Visible;
                var progress = new Progress<TranscriptionResult>(result =>
                {
                    ProgressBarMaxValue = result.TotalFileCount;
                    ProgressBarValue = result.TranscribedFileCount;
                    ProgessFileName = $"Transcribing... {result.FilePath}";
                    ProgessSum = result.Completed ? "" : $"{result.TranscribedFileCount}/{result.TotalFileCount}";
                    Debug.WriteLine($"{result.FilePath}: {result.TotalFileCount}: {result.TranscribedFileCount}"); // Console output for debugging
                });

                if (GenerateSingleFile)
                {
                    await transcribeFilesService.TranscribeAndSaveSingleFileAsync(files, _selectedLanguage, null, null, progress);
                }
                else
                {
                    await transcribeFilesService.TranscribeAndSaveBatchAsync(files, _selectedLanguage, null, progress);
                }
            }
            catch (Exception ex)
            {
                ErrorMessageIsOpen = true;
                ErrorMessage = ex.Message;
                ErrorSeverity = InfoBarSeverity.Error;
                ProgressVisible = Visibility.Collapsed;
            }
        }
    }
}
