﻿namespace Audionomy.BL.DataModels
{
    public class SpeechSynhesisOptionsModel
    {
        public string LanguageCode { get; set; } = string.Empty;
        
        public string OutputFile { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public bool ExportTranscription { get; set; }
    }
}
