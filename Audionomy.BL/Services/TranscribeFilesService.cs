namespace Audionomy.BL.Services
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Interfaces;
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;
    using System.IO;
    using System.Threading;

    public class TranscribeFilesService : ITranscribeFilesService
    {
        private readonly IApplicationSettingsService _settingsService;

        public TranscribeFilesService(IApplicationSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task TranscribeAsync(List<FileInfo> wavFiles, SpeechTranscriptionOptionsModel options, IProgress<TranscriptionResultModel>? progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(options.Language))
            {
                throw new ArgumentException("Input language cannot be null or empty.", nameof(options.Language));
            }

            if (string.IsNullOrEmpty(options.OutputDirectory))
            {
                throw new ArgumentException("Output direcotry cannot be null or empty.", nameof(options.OutputDirectory));
            }

            wavFiles = wavFiles.FindAll(x => x.Extension.Equals(".wav", StringComparison.CurrentCultureIgnoreCase));
            if (wavFiles.Count == 0)
            {
                throw new ArgumentException("Input file list must contain at least one .wav file.", nameof(wavFiles));
            }


            var settings = await _settingsService.LoadSettingsAsync();
            var speechConfig = SpeechConfig.FromSubscription(settings.Key, settings.Region);
            speechConfig.SpeechRecognitionLanguage = options.Language;

            var totalFileCount = wavFiles.Count;
            var transcribedFileCount = 0;

            if (options.UseSingleOutputFile)
            {
                var outputFile = string.IsNullOrEmpty(options.OutputFileName)
                ? $"Transcription_{DateTime.Now:yyyyMMddHHmmssfff}.txt"
                : options.OutputFileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                ? options.OutputFileName
                : $"{options.OutputFileName}.txt";

                var outputPath = Path.Combine(options.OutputDirectory, outputFile);

                foreach (var file in wavFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    transcribedFileCount++;
                    if (file == null)
                    {
                        continue;
                    }

                    progress?.Report(new TranscriptionResultModel(file.Name, totalFileCount, transcribedFileCount));

                    var text = await AudioToTextAsync(speechConfig, file.FullName, cancellationToken);
                    
                    await using var streamWriter = new StreamWriter(outputPath, append: true);
                    await streamWriter.WriteLineAsync($"{file.Name}|{text}");

                    await Task.Delay(1000, cancellationToken);
                }
            }
            else
            {
                foreach (var file in wavFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    transcribedFileCount++;
                    if (file == null)
                    {
                        continue;
                    }

                    progress?.Report(new TranscriptionResultModel(file.Name, totalFileCount, transcribedFileCount));

                    var text = await AudioToTextAsync(speechConfig, file.FullName, cancellationToken);

                    await Task.Delay(500, cancellationToken);

                    var outputPath = Path.Combine(options.OutputDirectory, $"{Path.GetFileNameWithoutExtension(file.Name)}.txt");

                    await using var streamWriter = new StreamWriter(outputPath, false);
                    await streamWriter.WriteLineAsync(text);

                    await Task.Delay(1000, cancellationToken);
                }
            }

            progress?.Report(new TranscriptionResultModel("Completed", totalFileCount, transcribedFileCount, true));
        }

        private static async Task<string> AudioToTextAsync(SpeechConfig speechConfig, string audioFilePath, CancellationToken cancellationToken = default)
        {
            int retryCount = 1;

            for (int attempt = 0; attempt <= retryCount; attempt++)
            {
                using var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);
                using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
                cancellationToken.ThrowIfCancellationRequested();

                switch (speechRecognitionResult.Reason)
                {
                    case ResultReason.RecognizedSpeech:
                        return speechRecognitionResult.Text;
                    case ResultReason.NoMatch:
                        return $"NOMATCH: Speech could not be recognized.";
                    case ResultReason.Canceled:
                        var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                        if (cancellation.Reason == CancellationReason.Error && attempt < retryCount)
                        {
                            await Task.Delay(1000, cancellationToken);
                            continue;
                        }
                        return cancellation.Reason == CancellationReason.Error ?
                            $"CANCELED: Reason={cancellation.Reason}, ErrorCode={cancellation.ErrorCode}, ErrorDetails={cancellation.ErrorDetails}"
                            : $"CANCELED: Reason={cancellation.Reason}"; ;
                    default:
                        return $"Unknown ResultReason: {speechRecognitionResult.Reason}";
                }
            }
            return "Transctiption failed after retry.";
        }

        private static void ValidateOptionsAndSettings(ApplicationSettingsModel settings, SpeechSynhesisOptionsModel options)
        {
            if (string.IsNullOrEmpty(settings.Key) || string.IsNullOrEmpty(settings.Region))
            {
                throw new ArgumentException("Missing Azure Key and Location/Region.");
            }
            if (string.IsNullOrEmpty(options.LanguageCode))
            {
                throw new ArgumentException("The output language cannot be null or empty.", nameof(options.LanguageCode));
            }

            if (string.IsNullOrEmpty(options.OutputFile))
            {
                throw new ArgumentException("The output file cannot be null or empty.", nameof(options.OutputFile));
            }

            if (string.IsNullOrEmpty(options.Text))
            {
                throw new ArgumentException("The text cannot be null or empty.", nameof(options.Text));
            }


        }

        private static bool AreAllFilesInSameDirectory(List<FileInfo> files)
        {
            if (files == null || files.Count == 0)
                return true; // If the list is empty or null, we consider this as true.

            string? firstDirectory = files[0].DirectoryName;

            if (string.IsNullOrEmpty(firstDirectory))
            {
                throw new Exception("First file has no valid directory.");
            }

            // Check if all files have the same directory
            return files.All(file => file.DirectoryName == firstDirectory);
        }
    }
}