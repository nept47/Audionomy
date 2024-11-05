namespace Audionomy.BL.Interfaces
{
    using Audionomy.BL.DataModels;

    public interface ISpeechSynthesisService
    {
        Task ExportTransctiption(SpeechSynhesisOptionsModel options, IProgress<SpeechSynthesisResultModel>? progress);
        Task GenerateFile(SpeechSynhesisOptionsModel options, IProgress<SpeechSynthesisResultModel>? progress = null, CancellationToken cancellationToken = default);
    }
}