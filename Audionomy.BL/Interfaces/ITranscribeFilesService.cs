namespace Audionomy.BL.Interfaces
{
    using Audionomy.BL.DataModels;

    public interface ITranscribeFilesService
    {
        Task TranscribeAsync(List<FileInfo> wavFiles, SpeechTranscriptionOptionsModel options, IProgress<TranscriptionResultModel>? progress = null, CancellationToken cancellationToken = default);
    }
}