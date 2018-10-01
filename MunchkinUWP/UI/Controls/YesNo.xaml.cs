using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MunchkinUWP.UI.Controls
{
    internal sealed partial class YesNo : ContentDialog
    {
        string tempDisplayText = "";
        internal YesNo(string i_string)
        {
            this.InitializeComponent();
            tempDisplayText = i_string;
        }


    

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void DisplayText_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DisplayText.Text = tempDisplayText;
        }
    }
}
