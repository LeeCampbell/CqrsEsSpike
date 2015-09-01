using System.Windows;
using DealCapture.Client.Dashboards;
using DealCapture.Client.Repositories;
using DealCapture.Client.Repositories.EventStore;
using DealCapture.Client.Repositories.MessageBus;
using DealCapture.Client.Repositories.Server;

namespace DealCapture.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShowMainWindow();
        }

        private static void ShowMainWindow()
        {
            var eventStoreClient = new EventStoreClient();
            var messageBusClient = new MessageBusClient();
            var dealEntryCommandHander = new DealEntryCommandHandler(messageBusClient, eventStoreClient);
            var dealRepo = new DealRepository(eventStoreClient, messageBusClient);

            dealEntryCommandHander.Start();


            var window = new MainWindow();
            var dealCaptureVm = new ActiveDealDashboardViewModel(dealRepo);
            window.DataContext = dealCaptureVm;
            using (dealCaptureVm.Start())
            {
                window.ShowDialog();
            }

            dealEntryCommandHander.Dispose();
            messageBusClient.Dispose();
            eventStoreClient.Dispose();
        }
    }
}
