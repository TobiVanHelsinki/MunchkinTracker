using System;
using System.Collections.Generic;
using System.Linq;
using TAPPLICATION;
using TAPPLICATION.IO;
using TLIB.Code.Uwp;
using TLIB.IO;
using TLIB.PlatformHelper;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

using static MunchkinUWP.Model.Sound;

namespace MunchkinUWP.IO
{
    internal partial class SoundBoardIO
    {
        internal const string SoundFolder = "Assets\\Sounds\\";
        internal static async void PlaySound(eSoundName sound)
        {
            string FileToPlay;
            switch (sound)
            {
                case eSoundName.Chord:
                    FileToPlay = "chord.wav";
                    break;
                case eSoundName.badumtshh:
                    FileToPlay = "badumtshh.aif";
                    break;
                case eSoundName.WilhelmScream:
                    FileToPlay = "WilhelmScream.mp3";
                    break;
                    
                default:
                    FileToPlay = "";
                    break;
            }

#if __ANDROID__
#else
            MediaElement mysong = new MediaElement();
            Windows.Storage.StorageFile SoundFile;
            try
            {
                SoundFile = await UwpIO.GetFile(new FileInfoClass()
                {
                    Fileplace = Place.Assets,
                    Filename = FileToPlay,
                    Filepath = StringHelper.GetPrefix(PrefixType.AppPackageData) + SoundFolder
                }, eUser: UserDecision.ThrowError, eCreation: FileNotFoundDecision.NotCreate);
            }
            catch (Exception)
            {
                return;
            }
            mysong.SetSource(await SoundFile.OpenAsync(Windows.Storage.FileAccessMode.Read), SoundFile.ContentType);
            mysong.AudioCategory = Windows.UI.Xaml.Media.AudioCategory.SoundEffects;
            mysong.Play();

#endif
        }

    }
}
