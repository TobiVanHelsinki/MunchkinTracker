using MunchkinUWP.Model;
using MunchkinUWP.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TAPPLICATION;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
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
    internal sealed partial class SettingsPage : Page
    {
        Game ViewModel = AppModel.Instance.MainObject;
        readonly SettingsModel Settings = SettingsModel.Instance;
        readonly AppModel Model = AppModel.Instance;
        readonly string eMail = SharedConstants.APP_PUBLISHER_MAIL;
        readonly string Inhaber = SharedConstants.APP_PUBLISHER;
        readonly string AppVersionBuild = SharedConstants.APP_VERSION_BUILD_DELIM;
        readonly string AppReviewLink = SharedConstants.APP_STORE_REVIEW_LINK;
        readonly string AppKontaktEmail = SharedConstants.APP_PUBLISHER_MAILTO;
        readonly string MoreAppsLink = SharedConstants.APP_MORE_APPS;
        readonly string AppLink = SharedConstants.APP_STORE_LINK;
        readonly List<HelpEntry> Help = Constants.HelpList;

        internal SettingsPage()
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
            switch (Settings.THEME)
            {
                case ElementTheme.Default:
                    ThemeBox.SelectedIndex = 2;
                    break;
                case ElementTheme.Light:
                    ThemeBox.SelectedIndex = 1;
                    break;
                case ElementTheme.Dark:
                    ThemeBox.SelectedIndex = 0;
                    break;
                default:
                    break;
            }
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
        /// <summary>
        /// Select the right Notification Template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                try
                {
                    ((ListViewItem)(sender as ListView).ContainerFromItem(item)).ContentTemplate = Notification;
                }
                catch (Exception)
                {
                }
            }
            foreach (var item in e.AddedItems)
            {
                try
                {
                    ListViewItem o = ((ListViewItem)(sender as ListView).ContainerFromItem(item));
                    try
                    {
                        o.ContentTemplate = NotificationX;
                    }
                    catch (Exception)
                    {
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        /// <summary>
        /// To Show the right Exception template at notification listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentControl_Loaded(object sender, RoutedEventArgs e)
        {
            if ((sender as ContentControl).DataContext != null)
            {
                (sender as ContentControl).ContentTemplate = ExceptionTemplate;
            }
        }

        // Info Stuff ##########################################
        private async Task ComposeEmail(Windows.ApplicationModel.Contacts.Contact recipient,
            string messageBody,
            StorageFile attachmentFile)
        {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Body = messageBody;

            if (attachmentFile != null)
            {
                var stream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(attachmentFile);

                var attachment = new Windows.ApplicationModel.Email.EmailAttachment(
                    attachmentFile.Name,
                    stream);

                emailMessage.Attachments.Add(attachment);
            }

            var email = recipient.Emails.FirstOrDefault<Windows.ApplicationModel.Contacts.ContactEmail>();
            if (email != null)
            {
                var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient(email.Address);
                emailMessage.To.Add(emailRecipient);
            }

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);

        }

        private async void KontaktEmail_Click(object sender, RoutedEventArgs e)
        {
            Contact Tobi = new Contact();
            ContactEmail TobiMail = new ContactEmail();
            TobiMail.Address = "TobivanHelsinki@live.de";
            TobiMail.Kind = ContactEmailKind.Work;
            Tobi.Emails.Add(TobiMail);
            await this.ComposeEmail(Tobi, "Kontakt mit dem App-Entwickler aufnehmen", null);
        }

        // Settings Handler ####################################
        private void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((sender as ComboBox).SelectedItem as ComboBoxItem).Tag)
            {
                case "Dark":
                    SettingsModel.Instance.THEME = ElementTheme.Dark;
                    break;
                case "Light":
                    SettingsModel.Instance.THEME = ElementTheme.Light;
                    break;
                default:
                    SettingsModel.Instance.THEME = ElementTheme.Default;
                    break;
            }
        }
        // Reset Stuff ##########################################

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog Confirm;
            var res = ResourceLoader.GetForCurrentView();
            string ConfirmMunchkins = res.GetString("ConfirmMunchkins");
            string ConfirmStatistics = res.GetString("ConfirmStatistics");
            string ConfirmAll = res.GetString("ConfirmAll");

            switch (((Button)sender).Name)
            {
                case "DeleteMunchkins":
                    Confirm = new YesNo(ConfirmMunchkins);
                    Confirm.PrimaryButtonClick += new TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>(DeleteMunchkinsOK);
                    break;
                case "DeleteStatistics":
                    Confirm = new YesNo(ConfirmStatistics);
                    Confirm.PrimaryButtonClick += new TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>(DeleteStatisticsOK);
                    break;
                case "DeleteAll":
                    Confirm = new YesNo(ConfirmAll);
                    Confirm.PrimaryButtonClick += new TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>(DeleteAllOK);
                    break;
                default:
                    Confirm = new YesNo("Etwas ist schief gelaufen, nix wird geschehen.");
                    break;
            }
            Confirm.SecondaryButtonClick += new TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>(ContentDialog_SecondaryButtonClick);
            await Confirm.ShowAsync();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void DeleteMunchkinsOK(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.ResetMunchkins();
            GoBack(null, null);
        }

        private void DeleteStatisticsOK(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.ResetStatistics();
        }

        private void DeleteAllOK(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.ResetAppModel();
            GoBack(null, null);
        }

        private void ResetGame_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ResetGame();
            GoBack(null, null);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
        // ROME ###############################################################
        internal Dictionary<string, RemoteSystem> DeviceMap = new Dictionary<string, RemoteSystem>();
        internal ObservableCollection<RemoteSystem> DeviceList = new ObservableCollection<RemoteSystem>();
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

    }
}
