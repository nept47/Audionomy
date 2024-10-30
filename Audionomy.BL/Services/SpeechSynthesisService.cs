namespace Audionomy.BL.Services
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Interfaces;
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;

    public class SpeechSynthesisService
    {
        private readonly ISettingsService<SecureSettingsModel> _settingsService;

        public SpeechSynthesisService(ISettingsService<SecureSettingsModel> settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task GenerateFile(SpeechSynhesisOptions speechSynhesisOptions, IProgress<TranscriptionResult>? progress = null, CancellationToken cancellationToken = default)
        {
            ValidateOptions(speechSynhesisOptions);

            var settings = _settingsService.LoadSettings();

            if (string.IsNullOrEmpty(settings.AzureSpeechServiceKey) || string.IsNullOrEmpty(settings.AzureSpeechServiceLocation))
            {
                throw new ArgumentException("Missing Azure Key and Location/Region.");
            }

            var speechConfig = SpeechConfig.FromSubscription(settings.AzureSpeechServiceKey, settings.AzureSpeechServiceLocation);

            using var audioConfig = AudioConfig.FromWavFileOutput(speechSynhesisOptions.OutputFile);
            speechConfig.SpeechSynthesisLanguage = speechSynhesisOptions.LanguageCode;
            // speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";
            using var speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig);
            await speechSynthesizer.SpeakTextAsync(speechSynhesisOptions.Text);
            if (speechSynhesisOptions.ExportTranscription)
            {
                await using var outputFile = new StreamWriter(speechSynhesisOptions.OutputFile.Substring(0, speechSynhesisOptions.OutputFile.Length-3) + "txt", false);
                await outputFile.WriteLineAsync(speechSynhesisOptions.Text);
            }
        }

        private static void ValidateOptions(SpeechSynhesisOptions speechSynhesisOptions)
        {
            if (string.IsNullOrEmpty(speechSynhesisOptions.LanguageCode))
            {
                throw new ArgumentException("The output language cannot be null or empty.", nameof(speechSynhesisOptions.LanguageCode));
            }

            if (string.IsNullOrEmpty(speechSynhesisOptions.OutputFile))
            {
                throw new ArgumentException("The output file cannot be null or empty.", nameof(speechSynhesisOptions.OutputFile));
            }

            if (string.IsNullOrEmpty(speechSynhesisOptions.Text))
            {
                throw new ArgumentException("The text cannot be null or empty.", nameof(speechSynhesisOptions.Text));
            }
        }
    }
}
