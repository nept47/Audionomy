namespace Audionomy.Views.Pages
{
    using Audionomy.BL.DataModels;
    using Audionomy.ViewModels.Pages;
    using Wpf.Ui.Controls;

    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        public SettingsViewModel ViewModel { get; }

        private void AddLanguage_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var language = ((e.Source as ListView).SelectedItem as VoiceLanguageModel);
            ViewModel.AddActiveLanguage(language).ConfigureAwait(false);
        }

        private void RevoveLanguage_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var language = ((e.Source as ListView).SelectedItem as VoiceLanguageModel);
            ViewModel.RemoveActiveLanguage(language).ConfigureAwait(false);
        }
    }
}
