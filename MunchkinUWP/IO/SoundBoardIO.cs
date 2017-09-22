﻿using System;
using TLIB;
using TLIB.IO;
using Windows.UI.Xaml.Controls;

using static MunchkinUWP.Model.Sound;

namespace MunchkinUWP.IO
{
    public partial class SoundBoardIO
    {
        public const string SoundFolder = "Assets\\Sounds\\";
        public static async void PlaySound(eSoundName sound)
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
                SoundFile = await WinIO.GetFile(new FileInfoClass()
                {
                    Fileplace = Place.Assets,
                    Filename = FileToPlay,
                    Filepath = CrossPlatformHelper.GetPrefix(CrossPlatformHelper.PrefixType.AppPackageData) + SoundFolder
                }, eUser: UserDecision.ThrowError, eCreation: FileNotFoundDecision.NotCreate);
            }
            catch (Exception ex)
            {
                return;
            }
            mysong.SetSource(await SoundFile.OpenAsync(Windows.Storage.FileAccessMode.Read), SoundFile.ContentType);
            mysong.Play();

#endif
        }

    }
}