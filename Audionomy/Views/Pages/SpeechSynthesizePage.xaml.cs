namespace Audionomy.Views.Pages
{
    using Audionomy.ViewModels.Pages;
    using System.Windows.Controls;
    using Wpf.Ui.Controls;

    /// <summary>
    /// Interaction logic for SynthesizePage.xaml
    /// </summary>
    public partial class SpeechSynthesizePage : INavigableView<SpeechSynthesizeViewModel>
    {
        public SpeechSynthesizePage(SpeechSynthesizeViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        public SpeechSynthesizeViewModel ViewModel { get; }
    }
}
