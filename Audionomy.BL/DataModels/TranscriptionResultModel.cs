namespace Audionomy.BL.DataModels
{
    public class TranscriptionResultModel
    {
        public string FilePath { get; set; } = string.Empty;
        
        public int TotalFileCount { get; set; }

        public int TranscribedFileCount { get; set; }

        public bool Completed { get; set; }

        public TranscriptionResultModel(string filePath, int totalFileCount, int transcribedFileCount, bool completed = false)
        {
            FilePath = filePath;
            TotalFileCount = totalFileCount;
            TranscribedFileCount = transcribedFileCount;
            Completed = completed;
        }
    }
}