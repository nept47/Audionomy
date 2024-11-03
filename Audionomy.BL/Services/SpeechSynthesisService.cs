namespace Audionomy.BL.Services
{
    using Audionomy.BL.DataModels;
    using Audionomy.BL.Extensions;
    using Audionomy.BL.Interfaces;
    using Audionomy.BL.Utilities;
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;

    public class SpeechSynthesisService : ISpeechSynthesisService
    {
        private readonly IApplicationSettingsService _settingsService;

        public SpeechSynthesisService(IApplicationSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task GenerateFile(SpeechSynhesisOptionsModel speechSynhesisOptions, IProgress<SpeechSynthesisResultModel>? progress = null, CancellationToken cancellationToken = default)
        {
            var settings = await _settingsService.LoadSettingsAsync();

            Validate(settings, speechSynhesisOptions);

            var fullFilename = speechSynhesisOptions.OutputFile.GetFullFilenameWithoutExtenstion();
            var tmpOutputFilename = string.Concat(fullFilename.AsSpan(0, fullFilename.Length - fullFilename.Length), "_tmp.wav");

            progress?.Report(new SpeechSynthesisResultModel(speechSynhesisOptions.OutputFile, "Synthesizing.", 1, 1));
            
            //await Task.Delay(3000);

            using (var audioConfig = AudioConfig.FromWavFileOutput(tmpOutputFilename))
            {
                var speechConfig = SpeechConfig.FromSubscription(settings.Key, settings.Region);
                speechConfig.SpeechSynthesisLanguage = speechSynhesisOptions.LanguageCode;

                // TODO: Investigate this speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";
                using (var speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig))
                {
                    await speechSynthesizer.SpeakTextAsync(speechSynhesisOptions.Text);
                }
            }
            progress?.Report(new SpeechSynthesisResultModel(speechSynhesisOptions.OutputFile, "Converting to Asterisk Format.", 1, 1));

            await AudioConverterUtility.ConvertToAsteriskFormatAsync(tmpOutputFilename, speechSynhesisOptions.OutputFile);

            //await Task.Delay(3000);

            if (speechSynhesisOptions.ExportTranscription)
            {
                progress?.Report(new SpeechSynthesisResultModel(speechSynhesisOptions.OutputFile, "Creating Transcription File.", 1, 1));
                await using var outputFile = new StreamWriter(speechSynhesisOptions.OutputFile.GetFullFilenameWithoutExtenstion() + ".txt", false);
                await outputFile.WriteLineAsync(speechSynhesisOptions.Text);
            }
            //await Task.Delay(3000);
            try
            {
                progress?.Report(new SpeechSynthesisResultModel(speechSynhesisOptions.OutputFile, "Removing temp file(s)...", 1, 1));
                File.Delete(tmpOutputFilename);
            }
            catch
            {
                // TODO: Handle the exception
            }
            //await Task.Delay(3000);
            progress?.Report(new SpeechSynthesisResultModel(speechSynhesisOptions.OutputFile, "Completed.", 1, 1, true));


        }

        private static void Validate(ApplicationSettingsModel settings, SpeechSynhesisOptionsModel speechSynhesisOptions)
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

            if (string.IsNullOrEmpty(settings.Key) || string.IsNullOrEmpty(settings.Region))
            {
                throw new ArgumentException("Missing Azure Key and Location/Region.");
            }
        }
    }
}
