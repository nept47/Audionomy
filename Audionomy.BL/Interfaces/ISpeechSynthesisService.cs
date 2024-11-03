using Audionomy.BL.DataModels;

namespace Audionomy.BL.Interfaces
{
    public interface ISpeechSynthesisService
    {
        Task ExportTransctiption(SpeechSynhesisOptionsModel speechSynhesisOptions, IProgress<SpeechSynthesisResultModel>? progress);
        Task GenerateFile(SpeechSynhesisOptionsModel speechSynhesisOptions, IProgress<SpeechSynthesisResultModel>? progress = null, CancellationToken cancellationToken = default);
    }
}