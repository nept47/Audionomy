namespace Audionomy.BL.DataModels
{
    public class SpeechSynthesisResultModel
    {
        public string FilePath { get; set; } = string.Empty;
        public string ProgressStep { get; set; } = string.Empty;
        public int TotalFileCount { get; set; }
        public int TranscribedFileCount { get; set; }
        public bool Completed { get; set; }
        public SpeechSynthesisResultModel(string filePath, string progressStep, int totalFileCount, int transcribedFileCount, bool completed = false)
        {
            FilePath = filePath;
            ProgressStep = progressStep;
            TotalFileCount = totalFileCount;
            TranscribedFileCount = transcribedFileCount;
            Completed = completed;
        }
    }
}
