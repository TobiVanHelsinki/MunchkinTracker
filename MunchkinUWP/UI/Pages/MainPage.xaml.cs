﻿using MunchkinUWP.Model;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using TLIB.Model;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

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
    public sealed partial class MainPage : Page
    {
        // Member Vars ====================================================================
        Game ViewModel = AppModel.Instance.MainObject;
        bool m_bDetailIsReloading;
        bool m_bMasterIsReloading;
        MunchkinOrder m_eOrderBeforeBattle = MunchkinOrder.undef;
        public MainPage()
        {
            RequestedTheme = SettingsModel.Instance.THEME;
            InitializeComponent();

#if DEBUG
            AppBarButton_Remote.Visibility = Visibility.Visible;
#else
            AppBarButton_Remote.Visibility = Visibility.Collapsed;
#endif

            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.CommandBar", "DefaultLabelPosition"))
            {
                CommandbarControl.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
                CommandbarMunchkin.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
                CommandbarOrder.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
                AppBarButton_Settings.LabelPosition = CommandBarLabelPosition.Collapsed;
                AppBarButton_Add.LabelPosition = CommandBarLabelPosition.Collapsed;
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
                ViewModel.lstMunchkin.CollectionChanged += LstMunchkin_CollectionChanged;
                AppModel.Instance.lstNotifications.CollectionChanged += (x, y) => ShowError(y);

                UpdateForVisualState(OverviewState);
            }

            async void ShowError(NotifyCollectionChangedEventArgs y)
            {
                foreach (Notification item in y.NewItems)
                {
                    if (!item.bIsRead)
                    {
                        var messageDialog = new MessageDialog(item.strMessage);
                        messageDialog.Commands.Add(new UICommand(
                            "OK"));
                        messageDialog.DefaultCommandIndex = 0;
                        await messageDialog.ShowAsync();
                        item.bIsRead = true;
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
            if (ViewModel.oCurrentMunchkin == null && ViewModel.lstMunchkin.Count != 0)
            {
                ViewModel.oCurrentMunchkin = ViewModel.lstMunchkin.First();
            }
            if (ViewModel.lstMunchkin.Count == 0)
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
            if (null == ViewModel.oCurrentMunchkin)
            {// to provide a xaml exception when pivot item is null
                UpdateForVisualState(OverviewState);
            }
        }

        void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e) => UpdateForVisualState(e.NewState);

        void UpdateForVisualState(VisualState newState)
        {
            if (ViewModel.oCurrentMunchkin != null && Window.Current.Bounds.Width >= 720)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                if (WideState != AdaptiveStates.CurrentState)
                {
                    VisualStateManager.GoToState(this, "WideState", false);
                }
                return;
            }

            if (newState == DetailState & ViewModel.oCurrentMunchkin != null)
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
            ((Munchkin)((Button)e.OriginalSource).DataContext).nLevel--;
        }
        void BtnGearLess_Click(object sender, RoutedEventArgs e)
        {
            ((Munchkin)((Button)e.OriginalSource).DataContext).nGear--;
        }
        void BtnLevelMore_Click(object sender, RoutedEventArgs e)
        {
            ((Munchkin)((Button)e.OriginalSource).DataContext).nLevel++;
        }
        void BtnGearMore_Click(object sender, RoutedEventArgs e)
        {
            ((Munchkin)((Button)e.OriginalSource).DataContext).nGear++;
        }
        void TextBoxNotes_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.oCurrentMunchkin.strNotes = ((TextBox)sender).Text;
        }
        void SwitchGender(object sender, TappedRoutedEventArgs e)
        {
            Munchkin.eGenderTyp eCurrentGender = ((Munchkin)(((ContentPresenter)sender).DataContext)).eGender;
            switch (eCurrentGender)
            {
                case Munchkin.eGenderTyp.m:
                    eCurrentGender = Munchkin.eGenderTyp.w;
                    ((ContentPresenter)sender).ContentTemplate = GenderFemale;
                    break;
                case Munchkin.eGenderTyp.w:
                    eCurrentGender = Munchkin.eGenderTyp.s;
                    ((ContentPresenter)sender).ContentTemplate = GenderNone;
                    break;
                case Munchkin.eGenderTyp.s:
                    eCurrentGender = Munchkin.eGenderTyp.m;
                    ((ContentPresenter)sender).ContentTemplate = GenderMale;
                    break;
                default:
                    eCurrentGender = Munchkin.eGenderTyp.s;
                    ((ContentPresenter)sender).ContentTemplate = GenderNone;
                    break;
            }
           ((Munchkin)(((ContentPresenter)sender).DataContext)).eGender = eCurrentGender;
        }
        void Gender_Loaded(object sender, RoutedEventArgs e)
        {
            switch (((Munchkin)(((ContentPresenter)sender).DataContext)).eGender)
            {
                case Munchkin.eGenderTyp.m:
                    ((ContentPresenter)sender).ContentTemplate = GenderMale;
                    break;
                case Munchkin.eGenderTyp.w:
                    ((ContentPresenter)sender).ContentTemplate = GenderFemale;
                    break;
                case Munchkin.eGenderTyp.s:
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
            switch (ViewModel.eCurrentOrder)
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
                MasterListView.ItemsSource = ViewModel.SetNewOrder(ViewModel.eCurrentOrder);
                MasterListView.SelectedItem = ViewModel.oCurrentMunchkin;
                m_bMasterIsReloading = false;
            }
        }

        void RefreshDetailView()
        {
            if (!m_bDetailIsReloading)
            {
                m_bDetailIsReloading = true;
                DetailPivotView.ItemsSource = ViewModel.lstMunchkin;
                DetailPivotView.SelectedItem = ViewModel.oCurrentMunchkin;
                m_bDetailIsReloading = false;
            }
        }

        void SelectEditState()
        {
            ApplyStyleToMasterView(MasterListViewState.Edit);
            m_eOrderBeforeBattle = ViewModel.eCurrentOrder;
            if (ViewModel.eCurrentOrder != MunchkinOrder.Reihe)
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
            if (ViewModel.oCurrentMunchkin != null && AdaptiveStates.CurrentState == OverviewState)
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
            ViewModel.SetNewOrder(MunchkinOrder.ABC == ViewModel.eCurrentOrder ? MunchkinOrder.ABC_Reverse : MunchkinOrder.ABC);
        }
        void Overview_OrderBtn_LvL_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetNewOrder(MunchkinOrder.LvL == ViewModel.eCurrentOrder ? MunchkinOrder.LvL_Reverse : MunchkinOrder.LvL);
        }
        void Overview_OrderBtn_Order_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetNewOrder(MunchkinOrder.Reihe);
            Overview_OrderBtn_Order.IsChecked = true;
        }
        void Overview_OrderBtn_Power_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetNewOrder(MunchkinOrder.Pwr == ViewModel.eCurrentOrder ? MunchkinOrder.Pwr_Reverse : MunchkinOrder.Pwr);
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
        
    }
}