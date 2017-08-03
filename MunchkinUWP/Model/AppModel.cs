using TLIB.Model;
using Windows.UI.Xaml;

namespace MunchkinUWP.Model
{
    public class AppModel : TLIB.Model.SharedAppModel<Game>
    {
        public AppModel() : base()
        {
        }
        public static AppModel Initialize()
        {
            if (instance == null)
            {
                instance = new AppModel();
            }
            return Instance;
        }
        public static AppModel Instance
        {
            get
            {
                return (AppModel)instance;
            }
        }
    }
}
