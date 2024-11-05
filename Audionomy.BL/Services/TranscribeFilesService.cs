namespace Audionomy.BL.Services
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Interfaces;
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;

    public class TranscribeFilesService : ITranscribeFilesService
    {
        private readonly IApplicationSettingsService _settingsService;

        public TranscribeFilesService(IApplicationSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task TranscribeAndSaveAsync(List<FileInfo> wavFiles, SpeechTranscriptionBaseOptionsModel options, IProgress<TranscriptionResultModel>? progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(options.Locate))
            {
                throw new ArgumentException("Input language cannot be null or empty.", nameof(options.Locate));
            }

            wavFiles = wavFiles.FindAll(x => x.Extension.Equals(".wav", StringComparison.CurrentCultureIgnoreCase));

            if (wavFiles == null || wavFiles.Count == 0)
            {
                throw new ArgumentException("Input file list cannot be null or empty.", nameof(wavFiles));
            }

            var settings = await _settingsService.LoadSettingsAsync();
            var speechConfig = SpeechConfig.FromSubscription(settings.Key, settings.Region);
            speechConfig.SpeechRecognitionLanguage = options.Locate;

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

                progress?.Report(new TranscriptionResultModel(file.Name, totalFileCount, transcribedFileCount));

                //await Task.Delay(3000);

                using var audioConfig = AudioConfig.FromWavFileInput(file.FullName);
                using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

                cancellationToken.ThrowIfCancellationRequested();

                var text = FileToText(speechRecognitionResult);

                var outputPath = options.OutputFolderPath ?? file.DirectoryName ?? throw new ArgumentException($"Output directory is invalid for file: {file.FullName}");
                var filePath = Path.Combine(outputPath, $"{Path.GetFileNameWithoutExtension(file.Name)}.txt");

                await using var outputFile = new StreamWriter(filePath, false);
                await outputFile.WriteLineAsync(text);
            }

            progress?.Report(new TranscriptionResultModel("Completed", totalFileCount, transcribedFileCount, true));
        }

        public async Task TranscribeAndSaveAsync(List<FileInfo> wavFiles, SpeechTranscriptionExtentOptionsModel option, IProgress<TranscriptionResultModel>? progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(option.Locate))
            {
                throw new ArgumentException("Input language cannot be null or empty.", nameof(option.Locate));
            }

            wavFiles = wavFiles.FindAll(x => x.Extension.Equals(".wav", StringComparison.CurrentCultureIgnoreCase));

            if (wavFiles == null || wavFiles.Count == 0)
            {
                throw new ArgumentException("Input file list cannot be null or empty.", nameof(wavFiles));
            }

            if (string.IsNullOrEmpty(option.OutputFolderPath) && !AreAllFilesInSameDirectory(wavFiles))
            {
                throw new ArgumentException($"Since not all files are in the same directory, {nameof(option.OutputFolderPath)} cannot be empty.");
            }

            var path = option.OutputFolderPath ?? wavFiles[0].DirectoryName ?? throw new ArgumentException("Output directory could not be determined.");

            option.OutputFilename = string.IsNullOrEmpty(option.OutputFilename)
                ? $"Transcription_{DateTime.Now:yyyyMMddHHmmssfff}.txt"
                : option.OutputFilename.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                ? option.OutputFilename
                : $"{option.OutputFilename}.txt";

            var filePath = Path.Combine(path, option.OutputFilename);

            var settings = await _settingsService.LoadSettingsAsync();
            var speechConfig = SpeechConfig.FromSubscription(settings.Key, settings.Region);
            speechConfig.SpeechRecognitionLanguage = option.Locate;
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

                progress?.Report(new TranscriptionResultModel(file.Name, totalFileCount, transcribedFileCount));

                //await Task.Delay(3000);

                using var audioConfig = AudioConfig.FromWavFileInput(file.FullName);
                using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

                cancellationToken.ThrowIfCancellationRequested();

                var text = FileToText(speechRecognitionResult);

                await using var outputFile = new StreamWriter(filePath, append: true);
                await outputFile.WriteLineAsync($"{file.Name}|{text}");
            }

            progress?.Report(new TranscriptionResultModel("Completed", totalFileCount, transcribedFileCount, true));
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

            string? firstDirectory = files[0].DirectoryName;

            if(string.IsNullOrEmpty(firstDirectory))
            {
                throw new Exception("First file has no valid directory.");
            }

            // Check if all files have the same directory
            return files.All(file => file.DirectoryName == firstDirectory);
        }
    }
}