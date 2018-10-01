using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MunchkinUWP.UI.Controls
{
    internal sealed partial class PivotHeader : UserControl
    {
        internal PivotHeader()
        {
            this.InitializeComponent();
        }

        internal string CustName
        {
            get { return (string)GetValue(CustNameProperty); }
            set { SetValue(CustNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustName.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty CustNameProperty =
            DependencyProperty.Register("CustName", typeof(string), typeof(PivotHeader), null);

        private void HeaderPath_Loaded(object sender, RoutedEventArgs e)
        {
            LineTop.EndPoint = new Windows.Foundation.Point(textblock.Width, 0);
            LineBottom.EndPoint = new Windows.Foundation.Point(textblock.Width, 40);
        }
    }
}
