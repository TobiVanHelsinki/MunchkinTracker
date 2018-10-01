using MunchkinUWP.Model;
using System.Collections.Generic;
using TAPPLICATION;
using TLIB.PlatformHelper;

namespace MunchkinUWP
{
    class Constants : SharedConstants
    {
        internal const string APP_NAME = "Munchkin Tracker";
        internal const string SAVE_GAME = "SAVE_GAME";
        internal const string FILE_VERSION = "1.0";
        internal const string APP_VERSION = "1.0";

        internal const string ELEMENT_THEME = "SETTINGS_ELEMENT_THEME";
        internal const string LISTVIEWSHORTMODE = "SETTINGS_LISTVIEWSHORTMODE";
        internal const string CONTAINER_SETTINGS_GAMEWARNINGS_LEVEL = "SETTINGS_GAMEWARNINGS_LEVEL";
        internal const string CONTAINER_SETTINGS_GAMEWARNINGS = "SETTINGS_GAMEWARNINGS";

        internal const uint STD_RANDOM_MAX = 6;
        internal const uint STD_RANDOMMAXTRIES = 100;
        internal const int STD_AUTOSAVE_INTERVAL = 2000;

        //========================================================================

        internal static List<HelpEntry> HelpList = new List<HelpEntry>() {
            new HelpEntry() { Paragraph = StringHelper.GetString("Help1_BlackBug"), Text = StringHelper.GetString("Help1") },
       };
    }
}
