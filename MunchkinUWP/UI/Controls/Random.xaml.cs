using MunchkinUWP.Model;
using TLIB;
using Windows.Security.Cryptography;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MunchkinUWP.UI.Controls
{
    public sealed partial class Random : ContentDialog
    {
        Game Model = AppModel.Instance.MainObject;
        private uint _nRandomResult;
        public uint nRandomResult
        {
            get
            {
                return _nRandomResult;
            }
            set
            {
                _nRandomResult = value;
                //NotifyPropertyChanged();
            }
        }


        public Random()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            RollDice();
            args.Cancel = true;
        }

        void Dice_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                ShowDice(Model.lstStatistics[Model.lstStatistics.Count-1].nResult);
            }
            catch (System.Exception)
            {
            }
        }

        void Dice_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RollDice();
        }

        void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        void RollDice()
        {
            nRandomResult = 0;
            uint nMaxTries = 0;
            while (nRandomResult == 0 && nMaxTries < Constants.STD_RANDOMMAXTRIES)
            {
                nRandomResult = CryptographicBuffer.GenerateRandomNumber();
                nRandomResult = nRandomResult % ((uint)Model.nRandomMax + 1);
                nMaxTries++;
            }
            ShowDice(nRandomResult);
            Model.lstStatistics.Add(new RandomResult(nRandomResult, 0));
        }

        void ShowDice(uint nRandomResult)
        {
            switch (nRandomResult)
            {
                case 1:
                    Dice.ContentTemplate = Eye1;
                    break;
                case 2:
                    Dice.ContentTemplate = Eye2;
                    break;
                case 3:
                    Dice.ContentTemplate = Eye3;
                    break;
                case 4:
                    Dice.ContentTemplate = Eye4;
                    break;
                case 5:
                    Dice.ContentTemplate = Eye5;
                    break;
                case 6:
                    Dice.ContentTemplate = Eye6;
                    break;
                default:
                    Dice.ContentTemplate = null;
                    Dice.ContentTemplate = EyeV;
                    this.nRandomResult = nRandomResult;
                    break;
            }
        }

        void EyeVTxT_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ((TextBlock)sender).Text = nRandomResult.ToString();
        }
    }
}
