using Audionomy.BL.DataModels;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Audionomy.BL.Services
{
    public class TranscribeFilesService
    {
        readonly private SpeechConfig _speechConfig;

        public TranscribeFilesService(string key, string region)
        {
            _speechConfig = SpeechConfig.FromSubscription(key, region);
        }

        public async Task TranscribeAndSaveBatchAsync(List<FileInfo> wavFiles, string language, string? outputDirectory, IProgress<TranscriptionResult>? progress = null)
        {
            if (string.IsNullOrEmpty(language))
            {
                throw new ArgumentException("Input language cannot be null or empty.", nameof(language));
            }

            _speechConfig.SpeechRecognitionLanguage = language;

            wavFiles = wavFiles.FindAll(x => x.Extension.Equals(".wav", StringComparison.CurrentCultureIgnoreCase));

            if (wavFiles == null || !wavFiles.Any())
            {
                throw new ArgumentException("Input file list cannot be null or empty.", nameof(wavFiles));
            }

            var totalFileCount = wavFiles.Count;
            var transcribedFileCount = 0;

            foreach (var file in wavFiles)
            {
                if (file == null || !file.Extension.Equals(".wav", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                transcribedFileCount++;

                progress?.Report(new TranscriptionResult(file.Name, totalFileCount, transcribedFileCount));

                //await Task.Delay(3000);

                using var audioConfig = AudioConfig.FromWavFileInput(file.FullName);
                using var speechRecognizer = new SpeechRecognizer(_speechConfig, audioConfig);
                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

                var outputPath = outputDirectory ?? file.DirectoryName
                    ?? throw new ArgumentException($"Output directory is invalid for file: {file.FullName}");

                var filePath = Path.Combine(outputPath, $"{Path.GetFileNameWithoutExtension(file.Name)}.txt");

                await using var outputFile = new StreamWriter(filePath, false);
                await outputFile.WriteLineAsync(FileToText(speechRecognitionResult));
            }

            progress?.Report(new TranscriptionResult("Completed", totalFileCount, transcribedFileCount, true));
        }

        public async Task TranscribeAndSaveSingleFileAsync(List<FileInfo> wavFiles, string language, string? outputDirectory, string? outputFilename, IProgress<TranscriptionResult>? progress = null)
        {
            if (string.IsNullOrEmpty(language))
            {
                throw new ArgumentException("Input language cannot be null or empty.", nameof(language));
            }

            _speechConfig.SpeechRecognitionLanguage = language;

            wavFiles = wavFiles.FindAll(x => x.Extension.Equals(".wav", StringComparison.CurrentCultureIgnoreCase));

            if (wavFiles == null || !wavFiles.Any())
            {
                throw new ArgumentException("Input file list cannot be null or empty.", nameof(wavFiles));
            }

            if (string.IsNullOrEmpty(outputDirectory) && !AreAllFilesInSameDirectory(wavFiles))
            {
                throw new ArgumentException($"Since not all files are in the same directory, {nameof(outputDirectory)} cannot be empty.");
            }

            var path = outputDirectory ?? wavFiles[0].DirectoryName ?? throw new ArgumentException("Output directory could not be determined.");

            outputFilename = string.IsNullOrEmpty(outputFilename)
                ? $"Transcription_{DateTime.Now:yyyyMMddHHmmssfff}.txt"
                : outputFilename.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                ? outputFilename
                : $"{outputFilename}.txt";

            var filePath = Path.Combine(path, outputFilename);

            var transcribedFileCount = 0;
            var totalFileCount = wavFiles.Count;

            foreach (var file in wavFiles)
            {
                var result = new TranscriptionResult(file.Name, totalFileCount, transcribedFileCount++);
                progress?.Report(result);

                if (file == null || file.Extension.ToLower() != ".wav")
                {
                    continue;
                }

                //await Task.Delay(3000);

                using var audioConfig = AudioConfig.FromWavFileInput(file.FullName);
                using var speechRecognizer = new SpeechRecognizer(_speechConfig, audioConfig);
                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

                await using var outputFile = new StreamWriter(filePath, append: true);
                await outputFile.WriteLineAsync($"{file.Name}|{FileToText(speechRecognitionResult)}");
            }

            var finalResult = new TranscriptionResult("Completed", totalFileCount, transcribedFileCount);
            progress?.Report(finalResult);
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