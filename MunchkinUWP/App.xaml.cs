using MunchkinUWP.IO;
using MunchkinUWP.Model;
using System;
using TAPPLICATION.Model;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MunchkinUWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private Windows.System.Display.DisplayRequest _displayRequest;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        internal App()
        {
            TLIB.PlatformHelper.ModelHelper.Platform = new TLIB.Code.Uwp.UwpModelHelper();
            TLIB.PlatformHelper.StringHelper.Platform = new TLIB.Code.Uwp.UwpStringHelper();

            TAPPLICATION.IO.SharedIO.CurrentIO = new TLIB.Code.Uwp.UwpIO();
            TAPPLICATION.Model.SharedSettingsModel.PlatformSettings = new TLIB.Code.Uwp.UwpSettings();

            AppModel.Initialize();
            SettingsModel.Initialize();
            this.InitializeComponent();
            _displayRequest = new Windows.System.Display.DisplayRequest();
            _displayRequest.RequestActive();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledExceptionAsync;
            UnhandledException += App_UnhandledExceptionAsync;

            SharedSettingsModel.Instance.FOLDERMODE = false;
            SharedSettingsModel.Instance.INTERN_SYNC = true;
        }
        // Startup ############################################################
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            try
            {
                AppModel.Instance.MainObject = await AppModelIO.LoadGame();
            }
            catch (Exception)
            {
                AppModel.Instance.MainObject = new Game();
            }

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                rootFrame.Navigated += OnNavigated;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

                // Register a handler for BackRequested events and set the
                // visibility of the Back button
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    rootFrame.CanGoBack ?
                    AppViewBackButtonVisibility.Visible :
                    AppViewBackButtonVisibility.Collapsed;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter

                    rootFrame.Navigate(typeof(Pages.MainPage));
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        // ShutDown ###########################################################
        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            try
            {
                await AppModelIO.SaveGame();
            }
            catch (Exception)
            {
            }
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        // Exception Handling #################################################


        async void App_UnhandledExceptionAsync(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            await AppModelIO.SaveGame();
            AppModel.Instance.lstNotifications.Add(new Notification(("Notification_Error_Unknown"), e.Exception));
            Current.Exit();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        // Navigation Handling #################################################
        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            // Each time a navigation event occurs, update the Back button's visibility
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame.CanGoBack)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }
    }
}
