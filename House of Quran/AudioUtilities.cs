using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace House_of_Quran
{
    internal static class AudioUtilities
    {
        internal static readonly System.Windows.Media.SolidColorBrush COLOR_AYAH_BEING_PLAYED = System.Windows.Media.Brushes.Red;
        internal static readonly System.Windows.Media.SolidColorBrush COLOR_AYAH = System.Windows.Media.Brushes.Black;

        internal static async Task PlayOggFileFromUrl(string lien, List<TextBlock> verseWords)
        {
            var req = System.Net.WebRequest.Create(lien);
            using (Stream stream = req.GetResponse().GetResponseStream())
            {
                using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(stream))
                using (var waveOut = new NAudio.Wave.WaveOutEvent())
                {
                    waveOut.PlaybackStopped += (sender, e) =>
                    {
                        if (verseWords != null)
                            verseWords.ForEach(x => x.Foreground = COLOR_AYAH);

                        PlayingAudioWaveOutEvent.Remove(waveOut);
                        waveOut.Dispose();
                        vorbisStream.Dispose();
                    };

                    waveOut.Init(vorbisStream);
                    waveOut.Play();
                    PlayingAudioWaveOutEvent.Add(waveOut);

                    if (verseWords != null)
                        verseWords.ForEach(x => x.Foreground = COLOR_AYAH_BEING_PLAYED);

                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        if (verseWords != null)
                            verseWords.ForEach(x => x.Foreground = COLOR_AYAH_BEING_PLAYED);
                        await Task.Delay(10);
                    }


                }
            }
        }


        internal static async Task PlayOggFromLocalFile(string file, List<TextBlock> verseWords)
        {
            using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(file))
            using (var waveOut = new NAudio.Wave.WaveOutEvent())
            {
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    if (verseWords != null)
                        verseWords.ForEach(x => x.Foreground = COLOR_AYAH);

                    PlayingAudioWaveOutEvent.Remove(waveOut);
                    waveOut.Dispose();
                    vorbisStream.Dispose();
                };

                waveOut.Init(vorbisStream);
                waveOut.Play();
                PlayingAudioWaveOutEvent.Add(waveOut);

                if (verseWords != null)
                    verseWords.ForEach(x => x.Foreground = COLOR_AYAH_BEING_PLAYED);

                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    if (verseWords != null)
                        verseWords.ForEach(x => x.Foreground = COLOR_AYAH_BEING_PLAYED);
                    await Task.Delay(10);
                }
            }
        }

        internal static void PlayMp3FromLocalFile(string file, List<TextBlock> verseWords = null)
        {
            Mp3FileReader Mp3Reader = new Mp3FileReader(file);
            var waveOut = new WaveOut();
            waveOut.Init(Mp3Reader);
            waveOut.Play();
            PlayingAudioWaveOut.Add(waveOut);

            if(verseWords != null)
                verseWords.ForEach(x => {x.Foreground = COLOR_AYAH_BEING_PLAYED; });

            waveOut.PlaybackStopped += (sender, e) =>
            {
                if (verseWords != null)
                    verseWords.ForEach(x => x.Foreground = COLOR_AYAH);

                PlayingAudioWaveOut.Remove(waveOut);
                waveOut.Dispose();
            };
        }

        private static List<WasapiOut> PlayingAudioWasapiOut = new List<WasapiOut>();
        private static List<WaveOut> PlayingAudioWaveOut = new List<WaveOut>();
        private static List<WaveOutEvent> PlayingAudioWaveOutEvent = new List<WaveOutEvent>();

        internal static async Task PlayAudioFromUrl(string url, List<TextBlock> verseWords)
        {
            using (var mf = new MediaFoundationReader(url))
            using (var wo = new WasapiOut())
            {
                wo.PlaybackStopped += (sender, e) =>
                {
                    PlayingAudioWasapiOut.Remove(wo);
                    wo.Dispose();
                    mf.Dispose();

                    if (verseWords != null)
                        verseWords.ForEach(x => x.Foreground = COLOR_AYAH);
                };

                wo.Init(mf);
                wo.Play();


                if (verseWords != null)
                    verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; });


                PlayingAudioWasapiOut.Add(wo);
                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    if (verseWords != null)
                        verseWords.ForEach(x => x.Foreground = COLOR_AYAH_BEING_PLAYED);
                    await Task.Delay(10);
                }
            }
        }

        /// <summary>
        /// Pause tout les audios actuellement en cours
        /// </summary>
        internal static void PauseAllPlayingAudio()
        {
            PlayingAudioWasapiOut.ForEach(x => { x.Stop(); });
            PlayingAudioWaveOut.ForEach(x => { x.Stop(); });
            PlayingAudioWaveOutEvent.ForEach(x => { x.Stop(); });
            PlayingAudioWasapiOut.Clear();
            PlayingAudioWasapiOut.Clear();
            PlayingAudioWaveOutEvent.Clear();
        }
    }
}
