namespace Audionomy.helpers
{
    using Audionomy.ViewModels;
    using Wpf.Ui.Controls;

    public static class InformationMessageProvider
    {

        public static InfoMessageModel GetNoLanguagesSelectedMessage()
        {
            return new InfoMessageModel(
                "No languages selected",
                "Please go to Settings > Languages to choose your preferences.",
                InfoBarSeverity.Warning,
                false);
        }

        public static InfoMessageModel GetNoLanguagesSelectedOnSettingMessage()
        {
            return new InfoMessageModel(
                "No languages selected",
                "Please go to the  Languages tab to choose your preferred languages.",
                InfoBarSeverity.Warning,
                false);
        }

        public static InfoMessageModel GetMissingCredentialsMessage()
        {
            return new InfoMessageModel(
                "Azure credentials are required",
                "Please go to Settings > Azure Credentials to configure them before proceeding.",
                InfoBarSeverity.Warning,
                false);
        }

        public static InfoMessageModel GetGenericErrorMessage(string errorMessage)
        {
            return new InfoMessageModel(
                "Error",
                errorMessage,
                InfoBarSeverity.Error);
        }

        public static InfoMessageModel GetGenericWarningMessage(string warningMessage)
        {
            return new InfoMessageModel(
                "Warning",
                warningMessage,
                InfoBarSeverity.Error);
        }

        public static InfoMessageModel GetTranscriptionCanceledMessage()
        {
            return new InfoMessageModel(
                "Transcription(s) Canceled",
                "The transcription process has been canceled successfully.",
                InfoBarSeverity.Informational);
        }

        public static InfoMessageModel GetSynthesisCanceledMessage()
        {
            return new InfoMessageModel(
                "Synthesis Canceled",
                "The text-to-audio synthesis process has been canceled successfully.",
                InfoBarSeverity.Informational);
        }

        public static InfoMessageModel GetCredentialsSavedMessage()
        {
            return new InfoMessageModel(
                "Credentials Saved",
                "Azure credentials have been saved successfully.",
                InfoBarSeverity.Success);
        }

        public static InfoMessageModel GetCredentialsSavedNextStepMessage()
        {
            return new InfoMessageModel(
                "Credentials Saved",
                "Now, please select your language(s) in the next tab to start using the application.",
                InfoBarSeverity.Informational,
                false);
        }

        public static InfoMessageModel GetInvalidFileNameMessage()
        {
            return new InfoMessageModel(
                "Invalid File Name",
                "The file name you entered is invalid. Please use a valid file name and try again.",
                InfoBarSeverity.Warning,
                isClosable: true);
        }

        public static InfoMessageModel GetAudioFileGeneratedMessage()
        {
            return new InfoMessageModel(
                "Audio File Generated",
                "Your text has been successfully converted to audio.",
                InfoBarSeverity.Success);
        }

        public static InfoMessageModel GetEmptyTextMessage()
        {
            return new InfoMessageModel(
                "Empty Text",
                "The text cannot be empty. Please enter some text and try again.",
                InfoBarSeverity.Warning);
        }
        public static InfoMessageModel GetNoSelectLanguageMessage()
        {
            return new InfoMessageModel(
                "Language Selection Required",
                "Please select a language to continue.",
                InfoBarSeverity.Warning);
        }

        public static InfoMessageModel GetFolderNotSelectedMessage()
        {
            return new InfoMessageModel(
                "Folder Not Selected",
                "Please select a folder to continue.",
                InfoBarSeverity.Warning);
        }

        public static InfoMessageModel GetNoWavFilesInFolderMessage()
        {
            return new InfoMessageModel(
                "No WAV Files Found",
                "There are no WAV files in the selected folder. Please choose a different folder or add WAV files to this folder.",
                InfoBarSeverity.Warning);
        }

    }
}
