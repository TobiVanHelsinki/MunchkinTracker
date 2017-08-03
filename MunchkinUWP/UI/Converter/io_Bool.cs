using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace MunchkinUWP.UI.Converter
{
    class io_Bool : IValueConverter
    {
        #region IValueConverter Members 
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (parameter.ToString())
            {
                case "Visibility":
                    return ((bool)value) ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
                default:
                    throw new Exception();
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            switch (parameter.ToString())
            {
                case "Visibility":
                    return ((Windows.UI.Xaml.Visibility)value == Windows.UI.Xaml.Visibility.Visible) ? true : false;
                default:
                    throw new Exception();
            }
        }
        #endregion

    }
}
