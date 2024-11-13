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
            if (e.Source is ListView list && list.SelectedItem is VoiceLanguageModel languageModel)
            {
                ViewModel.AddActiveLanguage(languageModel).ConfigureAwait(false);
            }
        }

        private void RemoveLanguage_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.Source is ListView list && list.SelectedItem is VoiceLanguageModel languageModel)
            {
                ViewModel.RemoveActiveLanguage(languageModel).ConfigureAwait(false);
            }
        }

        private void AddLanguage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && e.Source is ListView list && list.SelectedItem is VoiceLanguageModel languageModel)
            {
                var l = list.SelectedIndex;
                ViewModel.AddActiveLanguage(languageModel).ConfigureAwait(false);
                list.SelectedIndex = l;
                list.Focus();
            }
        }

        private void RemoveLanguage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && e.Source is ListView list && list.SelectedItem is VoiceLanguageModel languageModel)
            {
                var l = list.SelectedIndex;
                ViewModel.RemoveActiveLanguage(languageModel).ConfigureAwait(false);
                list.SelectedIndex = l;
                list.Focus();
            }
        }
    }
}
