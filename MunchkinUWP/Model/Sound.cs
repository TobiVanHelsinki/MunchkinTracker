
namespace MunchkinUWP.Model
{

    public class Sound
    {
        public enum eSoundName
        {
            Chord = 1,
            Beep = 2,
            badumtshh = 3
        }

        public string Name;
        public string Description;
        public eSoundName SoundName;
    }
}
