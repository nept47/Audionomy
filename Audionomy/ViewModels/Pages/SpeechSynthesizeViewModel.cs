namespace Audionomy.ViewModels.Pages
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Extensions;
    using Audionomy.BL.Interfaces;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.Win32;
    using System.Collections.ObjectModel;
    using System.IO;
    using Wpf.Ui;
    using Wpf.Ui.Controls;

    public partial class SpeechSynthesizeViewModel : ObservableObject, INavigationAware
    {
        private readonly IUserSettingsService _userSettingsService;
        private readonly IApplicationSettingsService _applicationSettingsService;
        private readonly ISpeechSynthesisService _speechSynthesisService;
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationWindow _navigationWindow;
        private ApplicationSettingsModel _appSettings;
        private UserSettingsModel _userSettings;
        private bool _isInitialized;
        private string? _lastSelectedFolder;
        private string? _selectedTxtFileName;

        CancellationTokenSource _cts;

        [ObservableProperty]
        private string _sourceFolder = String.Empty;

        [ObservableProperty]
        private ObservableCollection<VoiceLanguageModel> _comboBoxLanguages;

        [ObservableProperty]
        private VoiceLanguageModel? _selectedLanguage = new VoiceLanguageModel();

        [ObservableProperty]
        private string _language = String.Empty;

        [ObservableProperty]
        private bool _generateTransriptionFile;

        [ObservableProperty]
        private string _textToSynthesize;

        [ObservableProperty]
        private ProgressViewModel _progress = new ProgressViewModel();

        [ObservableProperty]
        private ErrorViewModel _error = new ErrorViewModel();

        [ObservableProperty]
        private int _selectedLanguageIndex = 0;

        public SpeechSynthesizeViewModel(
           IUserSettingsService userSettingsService,
           IApplicationSettingsService applicationSettingsService,
           ISpeechSynthesisService speechSynthesisService,
           IServiceProvider serviceProvider)
        {
            _userSettingsService = userSettingsService;
            _applicationSettingsService = applicationSettingsService;
            _speechSynthesisService = speechSynthesisService;
            _serviceProvider = serviceProvider;
            _navigationWindow = (_serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow)!;
        }

        public void OnNavigatedFrom() { }

        public async void OnNavigatedTo()
        {
            _appSettings = await _applicationSettingsService.LoadSettingsAsync();
            ComboBoxLanguages = new ObservableCollection<VoiceLanguageModel>(_appSettings.ActiveLanguages);
            _userSettings = await _userSettingsService.LoadSettingsAsync();
            SelectedLanguageIndex = ComboBoxLanguages.Select((language, index) => new { Language = language, Index = index })
                .FirstOrDefault(x => x.Language.Locale == _userSettings.SpeechSynthesisSettings?.Language?.Locale)?.Index ?? 0;

            if (!_isInitialized)
            {
                GenerateTransriptionFile = _userSettings.SpeechSynthesisSettings.GenerateTranscriptionFile;

                _isInitialized = true;
            }
        }

        [RelayCommand]
        public async Task OnOpenFile()
        {
            Error = new ErrorViewModel();
            Progress = new ProgressViewModel();
            _selectedTxtFileName = null;

            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                Filter = "Text File(*.txt)|*.txt",
                InitialDirectory = _userSettings.SpeechSynthesisSettings.OpenFolderPath ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            // Clear current text
            TextToSynthesize = string.Empty;

            _userSettings.SpeechSynthesisSettings.OpenFolderPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
            await _userSettingsService.SaveSettingsAsync(_userSettings);

            // Check if the FileName is valid 
            if (openFileDialog.FileName.Length == 0)
            {
                return;
            }

            var fileInfo = new FileInfo(openFileDialog.FileName);
            _selectedTxtFileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);

            StreamReader sr = new StreamReader(openFileDialog.FileName);
            var line = sr.ReadLine();
            while (line != null)
            {
                TextToSynthesize += line;
                //Read the next line
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
            Console.ReadLine();
        }

        [RelayCommand]
        public async Task OnGenerateFile()
        {
            try
            {
                Error = new ErrorViewModel();

                _userSettings.SpeechSynthesisSettings.GenerateTranscriptionFile = GenerateTransriptionFile;
                _userSettings.SpeechSynthesisSettings.Language = SelectedLanguage;
                await _userSettingsService.SaveSettingsAsync(_userSettings);

                var textToSynthesize = TextToSynthesize?.Trim();

                if (string.IsNullOrEmpty(_appSettings.Key) || string.IsNullOrEmpty(_appSettings.Region))
                {
                    _navigationWindow.Navigate(typeof(Views.Pages.SettingsPage));
                    return;
                }

                if (string.IsNullOrEmpty(textToSynthesize))
                {
                    Error = new ErrorViewModel("The text can't be empty.", InfoBarSeverity.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(SelectedLanguage?.Locale))
                {
                    Error = new ErrorViewModel("Please select a language.", InfoBarSeverity.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new()
                {
                    FileName = _selectedTxtFileName,
                    DefaultExt = "wav",
                    Filter = "Waveform Audio File(*.wav)|*.wav",
                    AddExtension = false,
                    InitialDirectory = _userSettings.SpeechSynthesisSettings.SaveFolderPath ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveFileDialog.ShowDialog() != true)
                {
                    return;
                }

                var filePath = saveFileDialog.FileName;

                if (string.IsNullOrEmpty(filePath))
                {
                    Error = new ErrorViewModel("File name is invalid", InfoBarSeverity.Warning);
                    return;
                }

                _userSettings.SpeechSynthesisSettings.SaveFolderPath = filePath.GetFolderName();
                await _userSettingsService.SaveSettingsAsync(_userSettings);

                _cts = new CancellationTokenSource();

                SpeechSynhesisOptionsModel speechSynhesisOptions = new SpeechSynhesisOptionsModel()
                {
                    ExportTranscription = GenerateTransriptionFile,
                    LanguageCode = SelectedLanguage.Locale,
                    OutputFile = filePath,
                    Text = textToSynthesize
                };

                var progress = new Progress<SpeechSynthesisResultModel>(result =>
                {
                    if (result.Completed)
                    {
                        Progress = new ProgressViewModel(result.TotalFileCount, result.TranscribedFileCount, $"{result.FilePath}", string.Empty);
                    }
                    else
                    {
                        Progress = new ProgressViewModel(result.TotalFileCount, result.TranscribedFileCount, $"{result.FilePath}...{result.ProgressStep}", $"{result.TranscribedFileCount}/{result.TotalFileCount}");
                    }
                });

                await _speechSynthesisService.GenerateFile(speechSynhesisOptions, progress, _cts.Token);

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
                //ShowTranscribe = Visibility.Visible;
                //ShowCancelTranscribe = Visibility.Hidden;
            }


        }
    }
}
