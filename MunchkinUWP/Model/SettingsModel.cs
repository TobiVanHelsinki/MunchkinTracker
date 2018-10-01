using Windows.UI.Xaml;

namespace MunchkinUWP.Model
{
    class SettingsModel : TAPPLICATION.Model.SharedSettingsModel
    {

        internal bool GAMEWARNINGS
        {
            get => PlatformSettings.GetBoolRoaming(Constants.CONTAINER_SETTINGS_GAMEWARNINGS);
            set
            {
                PlatformSettings.SetRoaming(Constants.CONTAINER_SETTINGS_GAMEWARNINGS, value);
                Instance.NotifyPropertyChanged();
            }
        }

        internal int GAMEWARNINGS_LEVEL
        {
            get => PlatformSettings.GetIntRoaming(Constants.CONTAINER_SETTINGS_GAMEWARNINGS_LEVEL);
            set
            {
                PlatformSettings.SetRoaming(Constants.CONTAINER_SETTINGS_GAMEWARNINGS_LEVEL, value);
                Instance.NotifyPropertyChanged();
            }
        }
        internal ElementTheme THEME
        {
            get => (ElementTheme)PlatformSettings.GetIntRoaming(Constants.ELEMENT_THEME);
            set
            {
                PlatformSettings.SetRoaming(Constants.ELEMENT_THEME, (int)value);
                NotifyPropertyChanged();
            }
        }

        internal bool ListViewShortMode
        {
            get => PlatformSettings.GetBoolRoaming(Constants.LISTVIEWSHORTMODE);
            set
            {
                PlatformSettings.SetRoaming(Constants.LISTVIEWSHORTMODE, value);
                NotifyPropertyChanged();
            }
        }


        internal SettingsModel() : base()
        {
        }

        internal static new SettingsModel Initialize()
        {
            if (instance == null)
            {
                instance = new SettingsModel();
            }
            return Instance;
        }
        internal static new SettingsModel Instance
        {
            get
            {
                return (SettingsModel)instance;
            }
        }
        internal static new SettingsModel I
        {
            get
            {
                return (SettingsModel)instance;
            }
        }
    }
}
