using Windows.UI.Xaml;

namespace MunchkinUWP.Model
{
    public class SettingsModel : TLIB.Model.SharedSettingsModel
    {
        public ElementTheme THEME
        {
            get => (ElementTheme)PlatformSettings.getInt(Constants.ELEMENT_THEME);
            set
            {
                PlatformSettings.set(Constants.ELEMENT_THEME, value);
                NotifyPropertyChanged();
            }
        }

        public bool ListViewShortMode
        {
            get => PlatformSettings.getBool(Constants.LISTVIEWSHORTMODE);
            set
            {
                PlatformSettings.set(Constants.LISTVIEWSHORTMODE, value);
                NotifyPropertyChanged();
            }
        }


        public SettingsModel() : base()
        {
        }

        public static new SettingsModel Initialize()
        {
            if (instance == null)
            {
                instance = new SettingsModel();
            }
            return Instance;
        }
        public static new SettingsModel Instance
        {
            get
            {
                return (SettingsModel)instance;
            }
        }
        public static new SettingsModel I
        {
            get
            {
                return (SettingsModel)instance;
            }
        }
    }
}
