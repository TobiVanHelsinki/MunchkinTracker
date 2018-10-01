using System.Threading.Tasks;
using MunchkinUWP.Model;
using TAPPLICATION.IO;
using TLIB.IO;

namespace MunchkinUWP.IO
{
    public class AppModelIO : SharedIO<Game>
    {

        internal async static Task<Game> LoadGame()
        {
            return await Load(AppModel.SaveGamePlace);
        }
        internal async static Task SaveGame()
        {
            await Save(AppModel.Instance.MainObject, UserDecision.ThrowError, SaveType.Auto, AppModel.SaveGamePlace);
        }
    }
}