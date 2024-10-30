namespace Audionomy.BL.Services
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Interfaces;
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;

    public class TranscribeFilesService : ITranscribeFilesService
    {
        private readonly ISettingsService<SecureSettingsModel> _settingsService;

        public TranscribeFilesService(ISettingsService<SecureSettingsModel> settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task TranscribeAndSaveAsync(List<FileInfo> wavFiles, SpeechTranscriptionBaseOptions speechTranscriptionOptions, IProgress<TranscriptionResult>? progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(speechTranscriptionOptions.LanguageCode))
            {
                throw new ArgumentException("Input language cannot be null or empty.", nameof(speechTranscriptionOptions.LanguageCode));
            }

            wavFiles = wavFiles.FindAll(x => x.Extension.Equals(".wav", StringComparison.CurrentCultureIgnoreCase));

            if (wavFiles == null || wavFiles.Count == 0)
            {
                throw new ArgumentException("Input file list cannot be null or empty.", nameof(wavFiles));
            }

            var settings = _settingsService.LoadSettings();
            var speechConfig = SpeechConfig.FromSubscription(settings.AzureSpeechServiceKey, settings.AzureSpeechServiceLocation);
            speechConfig.SpeechRecognitionLanguage = speechTranscriptionOptions.LanguageCode;

            var totalFileCount = wavFiles.Count;
            var transcribedFileCount = 0;

            foreach (var file in wavFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                transcribedFileCount++;
                if (file == null)
                {
                    continue;
                }

                progress?.Report(new TranscriptionResult(file.Name, totalFileCount, transcribedFileCount));

                //await Task.Delay(3000);

                using var audioConfig = AudioConfig.FromWavFileInput(file.FullName);
                using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

                cancellationToken.ThrowIfCancellationRequested();

                var text = FileToText(speechRecognitionResult);

                var outputPath = speechTranscriptionOptions.OutputFolderPath ?? file.DirectoryName ?? throw new ArgumentException($"Output directory is invalid for file: {file.FullName}");
                var filePath = Path.Combine(outputPath, $"{Path.GetFileNameWithoutExtension(file.Name)}.txt");

                await using var outputFile = new StreamWriter(filePath, false);
                await outputFile.WriteLineAsync(text);
            }

            progress?.Report(new TranscriptionResult("Completed", totalFileCount, transcribedFileCount, true));
        }

        public async Task TranscribeAndSaveAsync(List<FileInfo> wavFiles, SpeechTranscriptionExtentOptions speechTranscriptionOptions, IProgress<TranscriptionResult>? progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(speechTranscriptionOptions.LanguageCode))
            {
                throw new ArgumentException("Input language cannot be null or empty.", nameof(speechTranscriptionOptions.LanguageCode));
            }

            wavFiles = wavFiles.FindAll(x => x.Extension.Equals(".wav", StringComparison.CurrentCultureIgnoreCase));

            if (wavFiles == null || wavFiles.Count == 0)
            {
                throw new ArgumentException("Input file list cannot be null or empty.", nameof(wavFiles));
            }

            if (string.IsNullOrEmpty(speechTranscriptionOptions.OutputFolderPath) && !AreAllFilesInSameDirectory(wavFiles))
            {
                throw new ArgumentException($"Since not all files are in the same directory, {nameof(speechTranscriptionOptions.OutputFolderPath)} cannot be empty.");
            }

            var path = speechTranscriptionOptions.OutputFolderPath ?? wavFiles[0].DirectoryName ?? throw new ArgumentException("Output directory could not be determined.");

            speechTranscriptionOptions.OutputFilename = string.IsNullOrEmpty(speechTranscriptionOptions.OutputFilename)
                ? $"Transcription_{DateTime.Now:yyyyMMddHHmmssfff}.txt"
                : speechTranscriptionOptions.OutputFilename.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                ? speechTranscriptionOptions.OutputFilename
                : $"{speechTranscriptionOptions.OutputFilename}.txt";

            var filePath = Path.Combine(path, speechTranscriptionOptions.OutputFilename);

            var settings = _settingsService.LoadSettings();
            var speechConfig = SpeechConfig.FromSubscription(settings.AzureSpeechServiceKey, settings.AzureSpeechServiceLocation);
            speechConfig.SpeechRecognitionLanguage = speechTranscriptionOptions.LanguageCode;
            var totalFileCount = wavFiles.Count;
            var transcribedFileCount = 0;

            foreach (var file in wavFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                transcribedFileCount++;
                if (file == null)
                {
                    continue;
                }

                progress?.Report(new TranscriptionResult(file.Name, totalFileCount, transcribedFileCount));

                //await Task.Delay(3000);

                using var audioConfig = AudioConfig.FromWavFileInput(file.FullName);
                using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

                cancellationToken.ThrowIfCancellationRequested();

                var text = FileToText(speechRecognitionResult);

                await using var outputFile = new StreamWriter(filePath, append: true);
                await outputFile.WriteLineAsync($"{file.Name}|{text}");
            }

            progress?.Report(new TranscriptionResult("Completed", totalFileCount, transcribedFileCount, true));
        }

        private static string FileToText(SpeechRecognitionResult speechRecognitionResult)
        {
            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    return speechRecognitionResult.Text;
                case ResultReason.NoMatch:
                    return $"NOMATCH: Speech could not be recognized.";
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                    return cancellation.Reason == CancellationReason.Error ?
                        $"CANCELED: Reason={cancellation.Reason}, ErrorCode={cancellation.ErrorCode}, ErrorDetails={cancellation.ErrorDetails}"
                        : $"CANCELED: Reason={cancellation.Reason}"; ;
                default:
                    return $"Unknown ResultReason: {speechRecognitionResult.Reason}";
            }
        }

        private static bool AreAllFilesInSameDirectory(List<FileInfo> files)
        {
            if (files == null || files.Count == 0)
                return true; // If the list is empty or null, we consider this as true.

            string firstDirectory = files[0].DirectoryName;

            // Check if all files have the same directory
            return files.All(file => file.DirectoryName == firstDirectory);
        }
    }
}