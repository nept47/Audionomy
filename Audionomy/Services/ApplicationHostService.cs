using Audionomy.BL.Interfaces;
using Audionomy.BL.Services;
using Audionomy.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows.Navigation;
using Wpf.Ui;

namespace Audionomy.Services
{
    /// <summary>
    /// Managed host of the application.
    /// </summary>
    public class ApplicationHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationSettingsService _applicationSettingsService;
        private INavigationWindow _navigationWindow;

        public ApplicationHostService(IServiceProvider serviceProvider,
            IApplicationSettingsService applicationSettingsService)
        {
            _serviceProvider = serviceProvider;
            _applicationSettingsService = applicationSettingsService;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await HandleActivationAsync();
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates main window during activation.
        /// </summary>
        private async Task HandleActivationAsync()
        {
            if (!System.Windows.Application.Current.Windows.OfType<MainWindow>().Any())
            {
                _navigationWindow = (
                    _serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow
                )!;
                _navigationWindow!.ShowWindow();

                var settings = _applicationSettingsService.LoadSettings();
                if (settings.RequiresConfiguration() || settings.ActiveLanguages.Count == 0)
                {
                    _navigationWindow.Navigate(typeof(Views.Pages.SettingsPage));
                }
                else
                {
                    _navigationWindow.Navigate(typeof(Views.Pages.SpeechSynthesizePage));
                }
            }

            await Task.CompletedTask;
        }
    }
}
