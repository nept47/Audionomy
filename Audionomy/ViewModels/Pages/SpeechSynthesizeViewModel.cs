namespace Audionomy.ViewModels.Pages;

using Audionomy.BL.DataModels;
using Audionomy.BL.Enumerable;
using Audionomy.BL.Extensions;
using Audionomy.BL.Interfaces;
using Audionomy.helpers;
using Audionomy.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Automation;
using Wpf.Ui;
using Wpf.Ui.Appearance;
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
    private string? _selectedTxtFileName;
    private TempSynthesizedFileModel _tempSynthesizedFile;
    private WaveOutEvent _outputDevice;
    private AudioFileReader _audioFile;
    private CancellationTokenSource _cts;

    [ObservableProperty]
    private bool _requiresConfiguration = false;

    [ObservableProperty]
    private string _sourceFolder = String.Empty;

    [ObservableProperty]
    private ObservableCollection<VoiceLanguageModel> _comboBoxLanguages;

    [ObservableProperty]
    private VoiceLanguageModel? _selectedLanguage = new VoiceLanguageModel();

    [ObservableProperty]
    private VoiceLanguageStyleModel? _selectedLanguageStyle = new VoiceLanguageStyleModel();

    [ObservableProperty]
    private AudioFormat _audioFormat = AudioFormat.Default;

    public IEnumerable<AudioFormat> AudioFormats => Enum.GetValues(typeof(AudioFormat)).Cast<AudioFormat>();

    [ObservableProperty]
    private string _language = String.Empty;

    [ObservableProperty]
    private bool _generateTransriptionFile;

    [ObservableProperty]
    private string _textToSynthesize;

    [ObservableProperty]
    private ProgressViewModel _progress = new ProgressViewModel();

    [ObservableProperty]
    private InfoMessageModel _synthesisInfoBar = new InfoMessageModel();

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

    public void OnNavigatedFrom()
    {
        DisposeAudioResources();
        DeleteTempAudionFileIfExist();
        _tempSynthesizedFile = new TempSynthesizedFileModel();
    }

    public async void OnNavigatedTo()
    {
        _appSettings = await _applicationSettingsService.LoadSettingsAsync();

        RequiresConfiguration = _appSettings.RequiresConfiguration();
        if (RequiresConfiguration)
        {
            SynthesisInfoBar = InformationMessageProvider.GetMissingCredentialsMessage();
            return;
        }
        else if (_appSettings.ActiveLanguages.Count == 0)
        {
            RequiresConfiguration = true;
            SynthesisInfoBar = InformationMessageProvider.GetNoLanguagesSelectedMessage();
            return;
        }
        else
        {
            SynthesisInfoBar = new InfoMessageModel();
        }

        ComboBoxLanguages = new ObservableCollection<VoiceLanguageModel>(_appSettings.ActiveLanguages);
        _userSettings = await _userSettingsService.LoadSettingsAsync();
        SelectedLanguageIndex = ComboBoxLanguages.Select((language, index) => new { Language = language, Index = index })
            .FirstOrDefault(x => x.Language.Locale == _userSettings.SpeechSynthesisSettings?.Language?.Locale)?.Index ?? 0;

        if (!_isInitialized)
        {
            GenerateTransriptionFile = _userSettings.SpeechSynthesisSettings.GenerateTranscriptionFile;
            SelectedLanguage = _userSettings.SpeechSynthesisSettings.Language;
            AudioFormat = _userSettings.SpeechSynthesisSettings.Format;
            SelectedLanguageStyle = _userSettings.SpeechSynthesisSettings.Voice;
            _isInitialized = true;
        }
    }

    [RelayCommand]
    public async Task OnOpenFile()
    {
        SynthesisInfoBar = new InfoMessageModel();
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
        DisposeAudioResources();
        try
        {
            SynthesisInfoBar = new InfoMessageModel();

            await SaveUserSelectionsAsync();

            var textToSynthesize = TextToSynthesize?.Trim();

            if (string.IsNullOrEmpty(_appSettings.Key) || string.IsNullOrEmpty(_appSettings.Region))
            {
                _navigationWindow.Navigate(typeof(Views.Pages.SettingsPage));
                return;
            }

            if (string.IsNullOrEmpty(textToSynthesize))
            {
                SynthesisInfoBar = InformationMessageProvider.GetEmptyTextMessage();
                return;
            }

            if (string.IsNullOrEmpty(SelectedLanguage?.Locale))
            {
                SynthesisInfoBar = InformationMessageProvider.GetNoSelectLanguageMessage();
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
                SynthesisInfoBar = InformationMessageProvider.GetInvalidFileNameMessage();
                return;
            }

            _userSettings.SpeechSynthesisSettings.SaveFolderPath = filePath.GetFolderName();
            await _userSettingsService.SaveSettingsAsync(_userSettings);

            _cts = new CancellationTokenSource();

            var speechSynhesisOptions = new SpeechSynhesisOptionsModel(textToSynthesize, SelectedLanguage.Locale, filePath, GenerateTransriptionFile, AudioFormat, SelectedLanguageStyle?.ShortName);

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

            if (!SynthesizedFileExists())
            {
                await _speechSynthesisService.GenerateFile(speechSynhesisOptions, progress, _cts.Token);
            }
            else
            {
                File.Move(_tempSynthesizedFile.FilePath, filePath, true);
                await _speechSynthesisService.ExportTransctiption(speechSynhesisOptions, progress);
            }

            SynthesisInfoBar = InformationMessageProvider.GetAudioFileGeneratedMessage();
            var task = CloseSpeechSynthesisInfoBar();
        }
        catch (OperationCanceledException)
        {
            SynthesisInfoBar = InformationMessageProvider.GetSynthesisCanceledMessage();
        }
        catch (Exception ex)
        {
            SynthesisInfoBar = InformationMessageProvider.GetGenericErrorMessage(ex.Message);
        }
        finally
        {
            Progress = new ProgressViewModel();
        }
    }

    [RelayCommand]
    public async Task OnGenerateTmpFile()
    {
        DisposeAudioResources();
        try
        {
            SynthesisInfoBar = new InfoMessageModel();

            await SaveUserSelectionsAsync();

            var textToSynthesize = TextToSynthesize?.Trim();

            if (string.IsNullOrEmpty(_appSettings.Key) || string.IsNullOrEmpty(_appSettings.Region))
            {
                _navigationWindow.Navigate(typeof(Views.Pages.SettingsPage));
                return;
            }

            if (string.IsNullOrEmpty(textToSynthesize))
            {
                SynthesisInfoBar = InformationMessageProvider.GetEmptyTextMessage();
                return;
            }

            if (SelectedLanguage == null || string.IsNullOrEmpty(SelectedLanguage?.Locale))
            {
                SynthesisInfoBar = InformationMessageProvider.GetNoSelectLanguageMessage();
                return;
            }


            if (!SynthesizedFileExists())
            {
                _tempSynthesizedFile = new TempSynthesizedFileModel()
                {
                    FilePath = Path.GetTempPath() + Guid.NewGuid() + ".wav",
                    Locale = SelectedLanguage.Locale,
                    Voice = SelectedLanguageStyle?.ShortName ?? SelectedLanguage.Voices.First().ShortName,
                    Text = textToSynthesize.Trim(),
                    Format = AudioFormat
                };

                _cts = new CancellationTokenSource();

                var speechSynhesisOptions = new SpeechSynhesisOptionsModel(textToSynthesize.Trim(), SelectedLanguage.Locale, _tempSynthesizedFile.FilePath, false, AudioFormat, SelectedLanguageStyle?.ShortName);

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

            if (File.Exists(_tempSynthesizedFile.FilePath))
            {
                _outputDevice = new WaveOutEvent();
                _audioFile = new AudioFileReader(_tempSynthesizedFile.FilePath);

                _outputDevice.Init(_audioFile);
                _outputDevice.Play();
            }
        }
        catch (OperationCanceledException)
        {
            SynthesisInfoBar = InformationMessageProvider.GetSynthesisCanceledMessage();
        }
        catch (Exception ex)
        {
            SynthesisInfoBar = InformationMessageProvider.GetGenericErrorMessage(ex.Message);
        }
        finally
        {
            Progress = new ProgressViewModel();
        }
    }

    partial void OnSelectedLanguageChanged(VoiceLanguageModel? oldValue, VoiceLanguageModel? newValue)
    {
        if (newValue != null)
        {
            SelectedLanguageStyle = newValue.Voices.FirstOrDefault();
        }
    }

    private void DisposeAudioResources()
    {
        if (_outputDevice != null)
        {
            _outputDevice.Dispose();
        }

        if (_audioFile != null)
        {
            _audioFile.Dispose();
        }
    }

    private void DeleteTempAudionFileIfExist()
    {
        if (_tempSynthesizedFile != null && _tempSynthesizedFile.FilePath != null && File.Exists(_tempSynthesizedFile.FilePath))
        {
            File.Delete(_tempSynthesizedFile.FilePath);
        }
    }

    private bool SynthesizedFileExists()
    {
        return _tempSynthesizedFile != null
            && _tempSynthesizedFile.Format == AudioFormat
            && _tempSynthesizedFile.Locale == SelectedLanguage?.Locale
            && _tempSynthesizedFile.Text == TextToSynthesize?.Trim()
            && _tempSynthesizedFile.Voice == SelectedLanguageStyle?.ShortName
            && File.Exists(_tempSynthesizedFile.FilePath);
    }

    private async Task SaveUserSelectionsAsync()
    {
        _userSettings.SpeechSynthesisSettings.GenerateTranscriptionFile = GenerateTransriptionFile;
        _userSettings.SpeechSynthesisSettings.Language = SelectedLanguage;
        _userSettings.SpeechSynthesisSettings.Format = AudioFormat;
        _userSettings.SpeechSynthesisSettings.Voice = SelectedLanguageStyle;

        await _userSettingsService.SaveSettingsAsync(_userSettings);
    }

    private async Task CloseSpeechSynthesisInfoBar()
    {
        await Task.Delay(5000);
        SynthesisInfoBar = new InfoMessageModel();
    }
}
