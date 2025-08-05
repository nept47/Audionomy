namespace Audionomy.BL.Services;

using Audionomy.BL.DataModels;
using Audionomy.BL.Enumerable;
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

    public async Task GenerateFile(SpeechSynhesisOptionsModel options, IProgress<SpeechSynthesisResultModel>? progress = null, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsService.LoadSettingsAsync();
        ValidateOptionsAndSettings(settings, options);

        var tmpOutputFilename = options.Format == AudioFormat.Default
            ? options.OutputFile
            : $"{options.OutputFile.GetFullFilenameWithoutExtenstion()}_tmp.wav";

        progress?.Report(new SpeechSynthesisResultModel(options.OutputFile, "Synthesizing.", 1, 1));

        try
        {
            using (var audioConfig = AudioConfig.FromWavFileOutput(tmpOutputFilename))
            {
                var speechConfig = CreateSpeechConfig(options, settings);
                using var speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig);
                await speechSynthesizer.SpeakTextAsync(options.Text);
            }

            progress?.Report(new SpeechSynthesisResultModel(options.OutputFile, "Converting to Asterisk Format.", 1, 1));

            if (options.Format == AudioFormat.Asterisk)
            {
                await AudioConverterUtility.ConvertToAsteriskFormatAsync(tmpOutputFilename, options.OutputFile);
                TryDeleteTempFile(options, progress, tmpOutputFilename);
            }
            else if(options.Format == AudioFormat.AmazonConnect)
            {
                await AudioConverterUtility.ConvertToAmazonConnectFormatAsync(tmpOutputFilename, options.OutputFile);
                TryDeleteTempFile(options, progress, tmpOutputFilename);
            }

            if (options.ExportTranscription)
            {
                await ExportTransctiption(options, progress);
            }

            progress?.Report(new SpeechSynthesisResultModel(options.OutputFile, "Completed.", 1, 1, true));
        }
        catch (Exception ex)
        {
            progress?.Report(new SpeechSynthesisResultModel(options.OutputFile, $"Error: {ex.Message}", 1, 1, true));
            // TODO: Add logging
        }
    }

    public async Task ExportTransctiption(SpeechSynhesisOptionsModel options, IProgress<SpeechSynthesisResultModel>? progress)
    {
        progress?.Report(new SpeechSynthesisResultModel(options.OutputFile, "Creating Transcription File.", 1, 1));
        await using var outputFile = new StreamWriter($"{options.OutputFile.GetFullFilenameWithoutExtenstion()}.txt", false);
        await outputFile.WriteLineAsync(options.Text);
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

    private static SpeechConfig CreateSpeechConfig(SpeechSynhesisOptionsModel options, ApplicationSettingsModel settings)
    {
        var speechConfig = SpeechConfig.FromSubscription(settings.Key, settings.Region);
        speechConfig.SpeechSynthesisLanguage = options.LanguageCode;

        if (options.LanguageStyle != null)
        {
            speechConfig.SpeechSynthesisVoiceName = options.LanguageStyle;
        }

        return speechConfig;
    }

    private static void TryDeleteTempFile(SpeechSynhesisOptionsModel options, IProgress<SpeechSynthesisResultModel>? progress, string tmpOutputFilename)
    {
        try
        {
            progress?.Report(new SpeechSynthesisResultModel(options.OutputFile, "Removing temp file(s)...", 1, 1));
            File.Delete(tmpOutputFilename);
        }
        catch (Exception)
        {
            // TODO: Add logging
        }
    }

}
