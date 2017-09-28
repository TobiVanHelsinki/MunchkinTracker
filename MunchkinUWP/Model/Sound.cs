
namespace MunchkinUWP.Model
{

    public class Sound
    {
        public enum eSoundName
        {
            Chord = 10,
            Beep = 20,
            badumtshh = 30,
            spanishinq = 40,
            nein = 50,
            doch = 60,
            oh = 70,
            fliegersirene = 80,
            lachen = 90,
            WilhelmScream = 100
        }

        public string Name;
        public string Description;
        public string PicturePath;
        public eSoundName SoundName;
    }
}
