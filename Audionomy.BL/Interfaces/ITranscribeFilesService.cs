using Audionomy.BL.DataModels;

namespace Audionomy.BL.Interfaces
{
    public interface ITranscribeFilesService
    {
        Task TranscribeAndSaveAsync(List<FileInfo> wavFiles, SpeechTranscriptionBaseOptions speechTranscriptionOptions, IProgress<TranscriptionResult>? progress = null, CancellationToken cancellationToken = default);
        Task TranscribeAndSaveAsync(List<FileInfo> wavFiles, SpeechTranscriptionExtentOptions speechTranscriptionOptions, IProgress<TranscriptionResult>? progress = null, CancellationToken cancellationToken = default);
    }
}