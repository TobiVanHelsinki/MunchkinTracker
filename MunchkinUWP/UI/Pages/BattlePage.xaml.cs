using MunchkinUWP.Model;
using System;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MunchkinUWP.Pages
{
    internal sealed partial class BattlePage : Page
    {
        // Member Vars ====================================================================
        Game ViewModel = AppModel.Instance.MainObject;
        object STDAcrylicBrush;
        internal BattlePage()
        {
            //ThemeManager.ChangeTheme(new Uri("ms-appx:///Themes/PurpleMongooseTheme.xaml"));
            this.RequestedTheme = SettingsModel.Instance.THEME;
            this.InitializeComponent();
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.CommandBar", "DefaultLabelPosition"))
            {
                CommandbarBattle.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
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
        async void GoToPageRandom(object sender, RoutedEventArgs e)
        {
            MunchkinUWP.UI.Controls.Random RandomDialog = new UI.Controls.Random();
            await RandomDialog.ShowAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.RequestedTheme = SettingsModel.Instance.THEME;
            base.OnNavigatedTo(e);
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

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.Munchkin.CollectionChanged += (x,y) => lstMunchkinsBattle_UIRefresh();
            foreach (var item in ViewModel.Munchkin)
            {
                item.PropertyChanged -= (x, y) => lstMunchkinsBattle_UIRefresh();
                item.PropertyChanged += (x, y) => lstMunchkinsBattle_UIRefresh();
            }
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                try
                {
                    (this).Background = (AcrylicBrush)Resources["SystemControlAcrylicWindowBrush"];
                    //this.Background = (AcrylicBrush)STDAcrylicBrush;
                }
                catch (Exception)
                {
                }
            }
        }
        
        //############################################################################################
        // BAttle

        bool bIsInUpdate;

        void lstMunchkinsBattle_UIRefresh()
        {
            bIsInUpdate = true;
            for (int i = 0; i < ViewModel.Munchkin.Count(); i++)
            {
                if (ViewModel.Munchkin[i].IsBattle)
                {
                    //if (Munchkins.Visibility == Visibility.Visible)
                    {
                        Munchkins.SelectRange(new ItemIndexRange(i, 1));
                    }
                    //else
                    {
                        Munchkins2.SelectRange(new ItemIndexRange(i, 1));
                    }
                }
                else
                {
                    //if (Munchkins.Visibility == Visibility.Visible)
                    {
                        Munchkins.DeselectRange(new ItemIndexRange(i, 1));
                    }
                    //else
                    {
                        Munchkins2.DeselectRange(new ItemIndexRange(i, 1));
                    }
                }
            }
            bIsInUpdate = false;
        }

        void lstMunchkinsBattle_ModelRefresh()
        {
            if (!bIsInUpdate)
            {
                foreach (var item in ViewModel.Munchkin)
                {
                    if (Munchkins.Visibility == Visibility.Visible)
                    {
                        item.IsBattle = Munchkins.SelectedItems.Contains(item) ? true : false;
                    }
                    else
                    {
                        item.IsBattle = Munchkins2.SelectedItems.Contains(item) ? true : false;
                    }
                }
                lstMunchkinsBattle_UIRefresh();
            }
        }
        private void Munchkins_Loaded(object sender, RoutedEventArgs e) => lstMunchkinsBattle_UIRefresh();

        void SelectionChanged(object sender, SelectionChangedEventArgs e) => lstMunchkinsBattle_ModelRefresh();

        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //lstMunchkinsBattle_UIRefresh();
            if (e.PropertyName == "nMonsterPower" || e.PropertyName == "nMunchkinPower")
            {
                ChangeBattlePowerSizes();
            }
            if ( e.PropertyName == "bIsBattle")
            {
                lstMunchkinsBattle_UIRefresh();
            }
        }

        void BtnM_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MunchkinPowerMod++;
        }
        void BtnL_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MunchkinPowerMod--;
        }

        void MonsterPwrP1_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MonsterPower = ViewModel.MonsterPower + 1;
        }

        void MonsterPwrP3_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MonsterPower = ViewModel.MonsterPower + 3;
        }

        void MonsterPwrP5_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MonsterPower = ViewModel.MonsterPower + 5;
        }

        void MonsterPwrP10_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MonsterPower = ViewModel.MonsterPower + 10;
        }

        void MonsterPwrM1_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MonsterPower = ViewModel.MonsterPower - 1;
        }

        void MonsterPwrM3_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MonsterPower = ViewModel.MonsterPower - 3;
        }

        void MonsterPwrM5_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MonsterPower = ViewModel.MonsterPower - 5;
        }

        void MonsterPwrM10_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MonsterPower = ViewModel.MonsterPower - 10;
        }
        
        void GoBack(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
            }
        }
        async void GoToRandom(object sender, RoutedEventArgs e)
        {
            MunchkinUWP.UI.Controls.Random RandomDialog = new UI.Controls.Random();
            await RandomDialog.ShowAsync();
        }

        void ChangeBattlePowerSizes(object sender = null, RoutedEventArgs args = null)
        {
            int nMaxFontSize = 120;
            int nMinFontSize = 60;

            if (ViewModel.MunchkinPower < ViewModel.MonsterPower)
            {
                Munchkin_Power.FontSize = nMinFontSize;
                Monster_Power.FontSize = NewBattleSize(nMinFontSize, nMaxFontSize, ViewModel.MunchkinPower, ViewModel.MonsterPower);
            }
            else if (ViewModel.MunchkinPower > ViewModel.MonsterPower)
            {
                Munchkin_Power.FontSize = NewBattleSize(nMinFontSize, nMaxFontSize, ViewModel.MonsterPower, ViewModel.MunchkinPower);
                Monster_Power.FontSize = nMinFontSize;
            }
            else
            {
                Munchkin_Power.FontSize = nMinFontSize;
                Monster_Power.FontSize = nMinFontSize;
            }
        }
        static uint NewBattleSize(int min, int max, double smaller, double bigger)
        {
            double dRatio = bigger / smaller;

            double nNewSize = dRatio * min;

            if (nNewSize > max)
            {
                nNewSize = max;
            }
            nNewSize = Math.Round(nNewSize, 0);
            if (nNewSize <= min)
            {
                return (uint)min;
            }
            return (uint)nNewSize;
        }

        void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ResetBattle();
        }
        
    }
}
