﻿namespace Audionomy.BL.DataModels
{
    public class SpeechTranscriptionOptionsModel
    {
        public string? Language { get; set; }
        public string? OutputDirectory { get; set; }      
        public bool UseSingleOutputFile {  get; set; }
        public string? OutputFileName { get; set; }
    }
}
