using Audionomy.BL.DataModels;

namespace Audionomy.BL.Interfaces
{
    public interface ITranscribeFilesService
    {
        Task TranscribeAndSaveAsync(List<FileInfo> wavFiles, SpeechTranscriptionBaseOptionsModel options, IProgress<TranscriptionResultModel>? progress = null, CancellationToken cancellationToken = default);
        Task TranscribeAndSaveAsync(List<FileInfo> wavFiles, SpeechTranscriptionExtentOptionsModel options, IProgress<TranscriptionResultModel>? progress = null, CancellationToken cancellationToken = default);
    }
}