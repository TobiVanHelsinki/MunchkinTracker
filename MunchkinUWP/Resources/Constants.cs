using System.Collections.Generic;
using TLIB;
using TLIB.Model;

namespace MunchkinUWP
{
    public class Constants : TLIB.SharedConstants
    {
        public const string APP_NAME = "Munchkin Tracker";
        public const string SAVE_GAME = "SAVE_GAME";
        public const string FILE_VERSION = "1.0";
        public const string APP_VERSION = "1.0";

        public const string ELEMENT_THEME = "SETTINGS_ELEMENT_THEME";
        public const string LISTVIEWSHORTMODE = "SETTINGS_LISTVIEWSHORTMODE";
        public const string CONTAINER_SETTINGS_GAMEWARNINGS_LEVEL = "SETTINGS_GAMEWARNINGS_LEVEL";
        public const string CONTAINER_SETTINGS_GAMEWARNINGS = "SETTINGS_GAMEWARNINGS";

        public const uint STD_RANDOM_MAX = 6;
        public const uint STD_RANDOMMAXTRIES = 100;
        public const int STD_AUTOSAVE_INTERVAL = 5000;

        //========================================================================

        public const string APP_STORE_LINK = "ms-windows-store://pdp/?productid=9ncxwgx1kr8s";
        public const string APP_STORE_REVIEW_LINK = "ms-windows-store://review/?ProductId=9ncxwgx1kr8s";

        public static List<HelpEntry> HelpList = new List<HelpEntry>() {
            new HelpEntry() { Paragraph = CrossPlatformHelper.GetString("Help1_BlackBug"), Text = CrossPlatformHelper.GetString("Help1") },
       };
    }
}
