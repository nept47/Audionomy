using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace Audionomy.ViewModels.Pages
{
    public partial class SynthesizeViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private string _sourceFolder = String.Empty;

        [ObservableProperty]
        private string _language = String.Empty;

        [ObservableProperty]
        private bool _generateSingleFile;

        public void OnNavigatedFrom()
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedTo()
        {
            throw new NotImplementedException();
        }
    }
}
