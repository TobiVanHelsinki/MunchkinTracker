using System;
using System.ComponentModel;
using TAPPLICATION;
using TAPPLICATION.IO;
using TAPPLICATION.Model;
using TLIB.IO;
using Windows.ApplicationModel;

namespace MunchkinUWP.Model
{
    class AppModel : SharedAppModel<Game>
    {
        public static readonly FileInfoClass SaveGamePlace = new FileInfoClass(Place.Roaming, Constants.SAVE_GAME, SharedIO.CurrentIO.GetCompleteInternPath(Place.Roaming));

        internal AppModel() : base()
        {
        }
        internal static AppModel Initialize()
        {
            if (instance == null)
            {
                instance = new AppModel();
                SharedConstants.APP_VERSION_BUILD_DELIM = String.Format("{0}.{1}.{2}.{3}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);
                SharedConstants.APP_PUBLISHER_MAIL = "TobivanHelsinki@live.de";
                SharedConstants.APP_PUBLISHER = "Tobi van Helsinki";
                SharedConstants.APP_STORE_ID = "9NBLGGH40026";
            }
            return Instance;
        }
        internal static new AppModel Instance
        {
            get
            {
                return (AppModel)instance;
            }
        }
    }
}
