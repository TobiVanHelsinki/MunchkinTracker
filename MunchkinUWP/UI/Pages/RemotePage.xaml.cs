using MunchkinUWP.Model;
using MunchkinUWP.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TLIB_UWPFRAME;
using TLIB_UWPFRAME.Model;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System.RemoteSystems;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MunchkinUWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RemotePage : Page
    {
#pragma warning disable 0169    //  Proactively suppress unused field warning in case Bindings is not used.
        Game ViewModel = AppModel.Instance.MainObject;
        readonly SettingsModel Settings = SettingsModel.Instance;
        readonly AppModel Model = AppModel.Instance;
#pragma warning restore 0169

        public RemotePage()
        {
            this.RequestedTheme = SettingsModel.Instance.THEME;
            this.InitializeComponent();
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.CommandBar", "DefaultLabelPosition"))
            {
                CommandbarSettings.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
            }
        }
        
        // Nav Stuff ##########################################

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.RequestedTheme = SettingsModel.Instance.THEME;
            DeviceList = new ObservableCollection<RemoteSystem>();
            DeviceMap = new Dictionary<string, RemoteSystem>();
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
            }
        }

        // Gui Stuff ##########################################


        // ROME ###############################################################
        public Dictionary<string, RemoteSystem> DeviceMap = new Dictionary<string, RemoteSystem>();
        public ObservableCollection<RemoteSystem> DeviceList = new ObservableCollection<RemoteSystem>();
        private RemoteSystemWatcher m_remoteSystemWatcher;
        private void SearchCleanup()
        {
            if (m_remoteSystemWatcher != null)
            {
                m_remoteSystemWatcher.Stop();
                m_remoteSystemWatcher = null;
            }
            DeviceList.Clear();
            DeviceMap.Clear();
        }

        private async void Search_Clicked(object sender, RoutedEventArgs e)
        {
            // Disable the Search button while watcher is being started to avoid a potential
            // race condition of having two RemoteSystemWatchers running in parallel. 
            Button searchButton = sender as Button;
            searchButton.IsHitTestVisible = false;

            // Cleaning up any existing devices from previous searches.
            SearchCleanup();

            // Verify access for Remote Systems. 
            // Note: RequestAccessAsync needs to called from the UI thread.
            RemoteSystemAccessStatus accessStatus = await RemoteSystem.RequestAccessAsync();

            if (accessStatus == RemoteSystemAccessStatus.Allowed)
            {
                    SearchByRemoteSystemWatcher();
            }
            else
            {
                //todo throw error
                //UpdateStatus("Access to Remote Systems is " + accessStatus.ToString(), NotifyType.ErrorMessage);
            }

            searchButton.IsHitTestVisible = true;
        }

        private void SearchByRemoteSystemWatcher()
        {
            //if (FilterSearch.IsChecked.Value)
            //{
                // Build a watcher to continuously monitor for filtered remote systems.
                //m_remoteSystemWatcher = RemoteSystem.CreateWatcher(BuildFilters());
            //}
            //else
            //{
                // Build a watcher to continuously monitor for all remote systems.
                m_remoteSystemWatcher = RemoteSystem.CreateWatcher();
            //}

            // Subscribing to the event that will be raised when a new remote system is found by the watcher.
            m_remoteSystemWatcher.RemoteSystemAdded += RemoteSystemWatcher_RemoteSystemAdded;

            // Subscribing to the event that will be raised when a previously found remote system is no longer available.
            m_remoteSystemWatcher.RemoteSystemRemoved += RemoteSystemWatcher_RemoteSystemRemoved;

            // Subscribing to the event that will be raised when a previously found remote system is updated.
            m_remoteSystemWatcher.RemoteSystemUpdated += RemoteSystemWatcher_RemoteSystemUpdated;

            // Start the watcher.
            m_remoteSystemWatcher.Start();

            //todo display
            //UpdateStatus("Searching for devices...", NotifyType.StatusMessage);
            //DeviceListBox.Visibility = Visibility.Visible;
        }

        private async void RemoteSystemWatcher_RemoteSystemUpdated(RemoteSystemWatcher sender, RemoteSystemUpdatedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (DeviceMap.ContainsKey(args.RemoteSystem.Id))
                {
                    DeviceList.Remove(DeviceMap[args.RemoteSystem.Id]);
                    DeviceMap.Remove(args.RemoteSystem.Id);
                }
                DeviceList.Add(args.RemoteSystem);
                DeviceMap.Add(args.RemoteSystem.Id, args.RemoteSystem);
                //UpdateStatus("Device updated with Id = " + args.RemoteSystem.Id, NotifyType.StatusMessage);
            });
        }

        private async void RemoteSystemWatcher_RemoteSystemRemoved(RemoteSystemWatcher sender, RemoteSystemRemovedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (DeviceMap.ContainsKey(args.RemoteSystemId))
                {
                    DeviceList.Remove(DeviceMap[args.RemoteSystemId]);
                    //UpdateStatus(deviceMap[args.RemoteSystemId].DisplayName + " removed.", NotifyType.StatusMessage);
                    DeviceMap.Remove(args.RemoteSystemId);
                }
            });
        }

        private async void RemoteSystemWatcher_RemoteSystemAdded(RemoteSystemWatcher sender, RemoteSystemAddedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                DeviceList.Add(args.RemoteSystem);
                DeviceMap.Add(args.RemoteSystem.Id, args.RemoteSystem);
                //UpdateStatus(string.Format("Found {0} devices.", deviceList.Count), NotifyType.StatusMessage);
            });
        }

        //nachrichten austausch
        private async Task ConnectToRemoteAppServiceAsync()
        {
            RemoteSystem selectedDevice = null;// DeviceListComboBox.SelectedItem as RemoteSystem;
            if (selectedDevice != null)
            {
                // Create a remote system connection request.
                RemoteSystemConnectionRequest connectionRequest = new RemoteSystemConnectionRequest(selectedDevice);

                // Set up a new app service connection. The following app service name and package family name
                // are used in this sample to work with AppServices provider SDK sample on a remote system.
                using (AppServiceConnection connection = new AppServiceConnection
                {
                    AppServiceName = "com.microsoft.randomnumbergenerator",
                    PackageFamilyName = "Microsoft.SDKSamples.AppServicesProvider.CS_8wekyb3d8bbwe"
                })
                {
                    //UpdateStatus("Opening connection to remote app service...", NotifyType.StatusMessage);
                    AppServiceConnectionStatus status = await connection.OpenRemoteAsync(connectionRequest);

                    if (status == AppServiceConnectionStatus.Success)
                    {
                        //UpdateStatus("Successfully connected to remote app service.", NotifyType.StatusMessage);
                        await SendMessageToRemoteAppServiceAsync(connection);
                    }
                    else
                    {
                        //UpdateStatus("Attempt to open a remote app service connection failed with error - " + status.ToString(), NotifyType.ErrorMessage);
                    }
                }
            }
            else
            {
                //UpdateStatus("Select a device for remote connection.", NotifyType.ErrorMessage);
            }
        }

        private async Task SendMessageToRemoteAppServiceAsync(AppServiceConnection connection)
        {
            // Send message if connection to the remote app service is open.
            if (connection != null)
            {
                //Set up the inputs and send a message to the service.
                ValueSet inputs = new ValueSet();
                //inputs.Add("minvalue", m_minimumValue);
                //inputs.Add("maxvalue", m_maximumValue);
                //UpdateStatus("Sent message to remote app service. Waiting for a response...", NotifyType.StatusMessage);
                AppServiceResponse response = await connection.SendMessageAsync(inputs);

                if (response.Status == AppServiceResponseStatus.Success)
                {
                    if (response.Message.ContainsKey("result"))
                    {
                        string resultText = response.Message["result"].ToString();
                        if (string.IsNullOrEmpty(resultText))
                        {
                            //UpdateStatus("Remote app service did not respond with a result.", NotifyType.ErrorMessage);
                        }
                        else
                        {
                            //UpdateStatus("Result = " + resultText, NotifyType.StatusMessage);
                        }
                    }
                    else
                    {
                        //UpdateStatus("Response from remote app service does not contain a result.", NotifyType.ErrorMessage);
                    }
                }
                else
                {
                    //UpdateStatus("Sending message to remote app service failed with error - " + response.Status.ToString(), NotifyType.ErrorMessage);
                }
            }
            else
            {
                //UpdateStatus("Not connected to any app service. Select a device to open a connection.", NotifyType.ErrorMessage);
            }
        }


    }
}
