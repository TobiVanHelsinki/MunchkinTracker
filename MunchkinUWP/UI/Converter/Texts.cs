using System;
using Windows.UI.Xaml.Data;

namespace MunchkinUWP.UI.Converter
{
    class Texts : IValueConverter
    {
        #region IValueConverter Members 
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (null == parameter)
            {
                return "";
            }
            switch (parameter.ToString())
            {
                case "signedinteger":
                    if ((int)value < 0)
                    {
                        return value.ToString();
                    }
                    else if ((int)value > 0)
                    {
                        return "+" + value.ToString();
                    }
                    else
                    {
                        return "";
                    }

                default:
                    break;
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return 0;
        }
        #endregion

    }
}
