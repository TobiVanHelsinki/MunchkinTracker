using MunchkinUWP.Model;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using TAPPLICATION.Model;
using Windows.Foundation.Metadata;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using static MunchkinUWP.Model.Sound;

namespace MunchkinUWP.UI.Converter
{
    class CurrentMunchkinConverter : IValueConverter
    {
        #region IValueConverter Members 
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (Object)value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Munchkin)value;
        }
        #endregion

    }
}
namespace MunchkinUWP.Pages
{
    internal sealed partial class MainPage : Page
    {
        // Member Vars ====================================================================
        Game ViewModel = AppModel.Instance.MainObject;
        bool m_bDetailIsReloading;
        bool m_bMasterIsReloading;
        MunchkinOrder m_eOrderBeforeBattle = MunchkinOrder.undef;
        internal MainPage()
        {
            RequestedTheme = SettingsModel.Instance.THEME;
            InitializeComponent();

#if DEBUG
            AppBarButton_Remote.Visibility = Visibility.Visible;
            BtnSound.Visibility = Visibility.Visible;
#else
            AppBarButton_Remote.Visibility = Visibility.Collapsed;
            BtnSound.Visibility = Visibility.Collapsed;
#endif

            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.CommandBar", "DefaultLabelPosition"))
            {
                CommandbarControl.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
                CommandbarMunchkin.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
                CommandbarOrder.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
                AppBarButton_Settings.LabelPosition = CommandBarLabelPosition.Collapsed;
                AppBarButton_Add.LabelPosition = CommandBarLabelPosition.Collapsed;
            }
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                STDAcrylicBrush = new Windows.UI.Xaml.Media.AcrylicBrush()
                {
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                    FallbackColor = Windows.UI.Color.FromArgb(255, 220, 220, 220),
                    TintColor = Windows.UI.Color.FromArgb(200, 220, 220, 220),
                    Opacity = 100,
                    TintOpacity = 90,
                };
            }
        }
        // Navigation ======================================================================
        void AppBarButton_Remote_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RemotePage), null, new SuppressNavigationTransitionInfo());
        }

        void AppBarButton_Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage), ViewModel, new SuppressNavigationTransitionInfo());
        }

        void AppBarButton_Battle_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BattlePage), null, new SuppressNavigationTransitionInfo());
        }
        async void AppBarButton_Random_Click(object sender, RoutedEventArgs e)
        {
            UI.Controls.Random RandomDialog = new UI.Controls.Random();
            await RandomDialog.ShowAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.RequestedTheme = SettingsModel.Instance.THEME;
            base.OnNavigatedTo(e);
            //Handle Backbutton
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            if (e.NavigationMode == NavigationMode.Back)
            {
                UpdateForVisualState(AdaptiveStates.CurrentState);
            }
            //Handle Visual State things
            if (e.NavigationMode == NavigationMode.New)
            {
                ViewModel.OrderChanged += new PropertyChangedEventHandler(RefreshOrderUI);
                ViewModel.SetNewOrder(MunchkinOrder.Reihe);
                ViewModel.CurrentMunchkChanged += CurrentMunchkChanged;
                ViewModel.Munchkin.CollectionChanged += LstMunchkin_CollectionChanged;
                AppModel.Instance.lstNotifications.CollectionChanged += (x, y) => ShowError(y);

                UpdateForVisualState(OverviewState);
                LstMunchkin_CollectionChanged(null, null);
            }

            async void ShowError(NotifyCollectionChangedEventArgs y)
            {
                foreach (Notification item in y.NewItems)
                {
                    if (!item.IsRead)
                    {
                        var messageDialog = new MessageDialog(item.Message);
                        messageDialog.Commands.Add(new UICommand(
                            "OK"));
                        messageDialog.DefaultCommandIndex = 0;
                        await messageDialog.ShowAsync();
                        item.IsRead = true;
                    }
                }
            }

            //Do Phone Things
            if (ApiInformation.IsTypePresent(typeof(Windows.UI.ViewManagement.StatusBar).ToString()))
            {
                if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                {
                    try
                    {
                        var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                        var color = (Windows.UI.Color)this.Resources["SystemAccentColor"];
                        statusBar.BackgroundColor = color;
                        statusBar.ForegroundColor = Windows.UI.Colors.Black;
                    }catch (Exception){}
                }
            }
            //to preselect values at gui
            RefreshOrderUI();
            ApplyStyleToMasterView(MasterListViewState.Normal);
            RefreshDetailView();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;

        }
        void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (AdaptiveStates.CurrentState == DetailState)
            {
                UpdateForVisualState(OverviewState);
                e.Handled = true;
            }
        }
        // States ============================================================================
        void LstMunchkin_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ViewModel.CurrentMunchkin == null && ViewModel.Munchkin.Count != 0)
            {
                ViewModel.CurrentMunchkin = ViewModel.Munchkin.First();
            }
            if (ViewModel.Munchkin.Count == 0)
            {
                StartTipTxT.Visibility = Visibility.Visible;
            }
            else
            {
                StartTipTxT.Visibility = Visibility.Collapsed;
            }
        }

        void CurrentMunchkChanged(object sender, PropertyChangedEventArgs e)
        {
            if (null == ViewModel.CurrentMunchkin)
            {// to provide a xaml exception when pivot item is null
                UpdateForVisualState(OverviewState);
            }
        }

        void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e) => UpdateForVisualState(e.NewState);

        void UpdateForVisualState(VisualState newState)
        {
            if (ViewModel.CurrentMunchkin != null && Window.Current.Bounds.Width >= 720)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                if (WideState != AdaptiveStates.CurrentState)
                {
                    VisualStateManager.GoToState(this, "WideState", false);
                }
                return;
            }

            if (newState == DetailState & ViewModel.CurrentMunchkin != null)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                if (DetailState != AdaptiveStates.CurrentState)
                {
                    VisualStateManager.GoToState(this, "DetailState", false);
                }
                return;
            }
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed; // this causes bug when at other page
                if (OverviewState != AdaptiveStates.CurrentState)
                {
                    VisualStateManager.GoToState(this, "OverviewState", false);
                    RootView.OpenPaneLength = MainPageRoot.ActualWidth;
                }
                return;
            }
        }
        
        // Detail View UI Handling =======================================================
        void BtnLevelLess_Click(object sender, RoutedEventArgs e)
        {
            ((Munchkin)((Button)e.OriginalSource).DataContext).Level--;
        }
        void BtnGearLess_Click(object sender, RoutedEventArgs e)
        {
            ((Munchkin)((Button)e.OriginalSource).DataContext).Gear--;
        }
        void BtnLevelMore_Click(object sender, RoutedEventArgs e)
        {
            ((Munchkin)((Button)e.OriginalSource).DataContext).Level++;
        }
        void BtnGearMore_Click(object sender, RoutedEventArgs e)
        {
            ((Munchkin)((Button)e.OriginalSource).DataContext).Gear++;
        }
        void TextBoxNotes_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.CurrentMunchkin.Notes = ((TextBox)sender).Text;
        }
        void SwitchGender(object sender, TappedRoutedEventArgs e)
        {
            Munchkin.GenderTyp eCurrentGender = ((Munchkin)(((ContentPresenter)sender).DataContext)).Gender;
            switch (eCurrentGender)
            {
                case Munchkin.GenderTyp.m:
                    eCurrentGender = Munchkin.GenderTyp.w;
                    ((ContentPresenter)sender).ContentTemplate = GenderFemale;
                    break;
                case Munchkin.GenderTyp.w:
                    eCurrentGender = Munchkin.GenderTyp.s;
                    ((ContentPresenter)sender).ContentTemplate = GenderNone;
                    break;
                case Munchkin.GenderTyp.s:
                    eCurrentGender = Munchkin.GenderTyp.m;
                    ((ContentPresenter)sender).ContentTemplate = GenderMale;
                    break;
                default:
                    eCurrentGender = Munchkin.GenderTyp.s;
                    ((ContentPresenter)sender).ContentTemplate = GenderNone;
                    break;
            }
           ((Munchkin)(((ContentPresenter)sender).DataContext)).Gender = eCurrentGender;
        }
        void Gender_Loaded(object sender, RoutedEventArgs e)
        {
            switch (((Munchkin)(((ContentPresenter)sender).DataContext)).Gender)
            {
                case Munchkin.GenderTyp.m:
                    ((ContentPresenter)sender).ContentTemplate = GenderMale;
                    break;
                case Munchkin.GenderTyp.w:
                    ((ContentPresenter)sender).ContentTemplate = GenderFemale;
                    break;
                case Munchkin.GenderTyp.s:
                    ((ContentPresenter)sender).ContentTemplate = GenderNone;
                    break;
                default:
                    ((ContentPresenter)sender).ContentTemplate = GenderNone;
                    break;
            }
        }
        void Button_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((Button)sender).Height = ((Button)sender).ActualWidth;
        }
        // Master View ====================================================================

        void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (CurrentMasterListViewState != MasterListViewState.Edit)
            {
                UpdateForVisualState(DetailState);
            }
        }

        void RefreshOrderUI(object sender = null, EventArgs e = null)
        {
            RefreshMasterView(); // to refresh position of an element
            Overview_OrderBtn_Alpha.IsChecked = false;
            Overview_OrderBtn_LvL.IsChecked = false;
            Overview_OrderBtn_Order.IsChecked = false;
            Overview_OrderBtn_Power.IsChecked = false;
            switch (ViewModel.CurrentOrder)
            {
                case MunchkinOrder.ABC:
                    Overview_OrderBtn_Alpha.IsChecked = true;
                    break;
                case MunchkinOrder.LvL:
                    Overview_OrderBtn_LvL.IsChecked = true;
                    break;
                case MunchkinOrder.Pwr:
                    Overview_OrderBtn_Power.IsChecked = true;
                    break;
                case MunchkinOrder.ABC_Reverse:
                    Overview_OrderBtn_Alpha.IsChecked = true;
                    break;
                case MunchkinOrder.LvL_Reverse:
                    Overview_OrderBtn_LvL.IsChecked = true;
                    break;
                case MunchkinOrder.Pwr_Reverse:
                    Overview_OrderBtn_Power.IsChecked = true;
                    break;
                case MunchkinOrder.Reihe:
                    Overview_OrderBtn_Order.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        void RefreshMasterView()
        {
            if (!m_bMasterIsReloading)
            {
                m_bMasterIsReloading = true;
                MasterListView.ItemsSource = ViewModel.SetNewOrder(ViewModel.CurrentOrder);
                MasterListView.SelectedItem = ViewModel.CurrentMunchkin;
                m_bMasterIsReloading = false;
            }
        }

        void RefreshDetailView()
        {
            if (!m_bDetailIsReloading)
            {
                m_bDetailIsReloading = true;
                DetailPivotView.ItemsSource = ViewModel.Munchkin;
                DetailPivotView.SelectedItem = ViewModel.CurrentMunchkin;
                m_bDetailIsReloading = false;
            }
        }

        void SelectEditState()
        {
            ApplyStyleToMasterView(MasterListViewState.Edit);
            m_eOrderBeforeBattle = ViewModel.CurrentOrder;
            if (ViewModel.CurrentOrder != MunchkinOrder.Reihe)
            {
                ViewModel.SetNewOrder(MunchkinOrder.Reihe);
            }
            AppBarButton_Edit.Visibility = Visibility.Collapsed;
            AppBarButton_EditFinish.Visibility = Visibility.Visible;
            EditTipTxT.Visibility = Visibility.Visible;
            Overview_OrderBtn_Alpha.IsEnabled = false;
            Overview_OrderBtn_LvL.IsEnabled = false;
            Overview_OrderBtn_Order.IsEnabled = false;
            Overview_OrderBtn_Power.IsEnabled = false;
            MasterListView.AllowDrop = true;
            MasterListView.CanReorderItems = true;
        }

        void SelectDisplayState()
        {
            ApplyStyleToMasterView(MasterListViewState.Normal);
            if (m_eOrderBeforeBattle != MunchkinOrder.Reihe)
            {
                ViewModel.SetNewOrder(m_eOrderBeforeBattle);
            }
            m_eOrderBeforeBattle = MunchkinOrder.undef;
            AppBarButton_Edit.Visibility = Visibility.Visible;
            AppBarButton_EditFinish.Visibility = Visibility.Collapsed;
            EditTipTxT.Visibility = Visibility.Collapsed;
            Overview_OrderBtn_Alpha.IsEnabled = true;
            Overview_OrderBtn_LvL.IsEnabled = true;
            Overview_OrderBtn_Order.IsEnabled = true;
            Overview_OrderBtn_Power.IsEnabled = true;
            MasterListView.AllowDrop = false;
            MasterListView.CanReorderItems = false;
            if (ViewModel.CurrentMunchkin != null && AdaptiveStates.CurrentState == OverviewState)
            {
                UpdateForVisualState(WideState);
            }
        }
        // Master View State Handling ====================================================
        enum MasterListViewState
        {
            Normal = 1,
            Light = 2,
            Edit = 4,
        }
        MasterListViewState CurrentMasterListViewState = MasterListViewState.Normal;

        void ApplyStyleToMasterView(MasterListViewState newState)
        {
            if (newState == MasterListViewState.Light || newState == MasterListViewState.Normal)
            {
                CurrentMasterListViewState = SettingsModel.I.ListViewShortMode ? MasterListViewState.Light : MasterListViewState.Normal;
            }
            else
            {
                CurrentMasterListViewState = newState;
            }
            switch (CurrentMasterListViewState)
            {
                case MasterListViewState.Normal:
                    MasterListView.ItemTemplate = MasterListViewItemTemplate_Normal;
                    break;
                case MasterListViewState.Light:
                    MasterListView.ItemTemplate = MasterListViewItemTemplate_Light;
                    break;
                case MasterListViewState.Edit:
                    MasterListView.ItemTemplate = MasterListViewItemTemplate_Edit;
                    break;
                default:
                    break;
            }
        }

        // Master View UI Handling =========================================================
        void BtnOverviewAdd_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddMunchkin();
            SelectEditState();
            //ViewModel.oCurrentMunchkin = ViewModel.lstMunchkin.Last();
        }
        void BtnOverviewDelete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveMunchkin((Munchkin)((Button)e.OriginalSource).DataContext);
            //RefreshDetailView();
        }

        void Overview_OrderBtn_Alpha_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetNewOrder(MunchkinOrder.ABC == ViewModel.CurrentOrder ? MunchkinOrder.ABC_Reverse : MunchkinOrder.ABC);
        }
        void Overview_OrderBtn_LvL_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetNewOrder(MunchkinOrder.LvL == ViewModel.CurrentOrder ? MunchkinOrder.LvL_Reverse : MunchkinOrder.LvL);
        }
        void Overview_OrderBtn_Order_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetNewOrder(MunchkinOrder.Reihe);
            Overview_OrderBtn_Order.IsChecked = true;
        }
        void Overview_OrderBtn_Power_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetNewOrder(MunchkinOrder.Pwr == ViewModel.CurrentOrder ? MunchkinOrder.Pwr_Reverse : MunchkinOrder.Pwr);
        }

        void AppBarButton_EditFinish_Click(object sender, RoutedEventArgs e) => SelectDisplayState();
        void AppBarButton_Edit_Click(object sender, RoutedEventArgs e) => SelectEditState();


        // new stuff ================================================================

        void RootView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (OverviewState == AdaptiveStates.CurrentState)
            {
                RootView.OpenPaneLength = MainPageRoot.ActualWidth;
            }
        }

        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            IO.SoundBoardIO.PlaySound((eSoundName)(sender as Button).DataContext);
        }

        #region fuent design
        private void MasterListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                try
                {
                    //(sender as ListView).Background = (AcrylicBrush)STDAcrylicBrush;
                    (sender as ListView).Background = (AcrylicBrush)Resources["SystemControlAcrylicWindowBrush"];
                }
                catch (Exception)
                {
                }
            }
        }

        object STDAcrylicBrush;

        private void DetailPivotView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                try
                {
                    (sender as Pivot).Background = (AcrylicBrush)Resources["SystemControlAcrylicWindowBrush"];
                    //(sender as Pivot).Background = (AcrylicBrush)STDAcrylicBrush;
                }
                catch (Exception)
                {
                }
            }
        }
        #endregion
    }
}
