using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using Brushes = System.Windows.Media.Brushes;

namespace House_of_Quran
{
    internal static class AudioUtilities
    {
        internal static readonly System.Windows.Media.SolidColorBrush COLOR_AYAH_BEING_PLAYED = System.Windows.Media.Brushes.Red;
        internal static readonly System.Windows.Media.SolidColorBrush COLOR_AYAH = System.Windows.Media.Brushes.Black;
        internal static List<TextBlock> LastColoredTextBlock;

        internal static double LastAudioPlayedDuration = 0;

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
                        LastAudioPlayedDuration = vorbisStream.TotalTime.TotalMilliseconds;

                        if (verseWords != null)
                            verseWords.ForEach(x => { x.Foreground = COLOR_AYAH;x.Background = Brushes.Transparent; }) ;

                        waveOut.Dispose();
                        vorbisStream.Dispose();

                        PlayNextAudioIfNeeded();

                    };

                    waveOut.Init(vorbisStream);
                    waveOut.Play();
                    woE = waveOut;

                    LastColoredTextBlock = verseWords;

                    if (verseWords != null)
                        verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background = MainWindow._MainWindow.checkBox_tajweed.IsChecked.Value ? Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });

                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        if (verseWords != null)
                            verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background = MainWindow._MainWindow.checkBox_tajweed.IsChecked.Value ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });
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
                    LastAudioPlayedDuration = vorbisStream.TotalTime.TotalMilliseconds;

                    if (verseWords != null)
                        verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });

                    waveOut.Dispose();
                    vorbisStream.Dispose();

                    PlayNextAudioIfNeeded();

                };

                waveOut.Init(vorbisStream);
                waveOut.Play();
                woE = waveOut;

                LastColoredTextBlock = verseWords;

                if (verseWords != null)
                    verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background = MainWindow._MainWindow.checkBox_tajweed.IsChecked.Value ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });

                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    if (verseWords != null)
                        verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background = MainWindow._MainWindow.checkBox_tajweed.IsChecked.Value ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });
                    await Task.Delay(10);
                }
            }
        }

        private static WaveOutEvent woE = new WaveOutEvent();
        private static WaveOut wo = new WaveOut();
        private static WasapiOut wao = new WasapiOut();

        private static bool PauseEvent = true;

        internal static void PauseAudio()
        {
            woE.Pause();
            wo.Pause();
            wao.Pause();
            ForcedStop = true;
        }

        internal static void PlayAudio()
        {
            try { woE.Play();  } catch { }
            try { wo.Play();  } catch { }
            try { wao.Play();  } catch { }         
        }

        internal static async void PlayMp3FromLocalFile(string file, List<TextBlock> verseWords = null)
        {
            Mp3FileReader Mp3Reader = new Mp3FileReader(file);
            var waveOut = new WaveOut();
            waveOut.Init(Mp3Reader);
            waveOut.Play();
            wo = waveOut;
            LastColoredTextBlock = verseWords;

            if (verseWords != null)
                verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background = MainWindow._MainWindow.checkBox_tajweed.IsChecked.Value ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });

            var t_keepColor = new System.Timers.Timer(10);

            if (verseWords != null)
            {
                t_keepColor.Elapsed += (sender, e) =>
                {
                    Application.Current.Dispatcher.BeginInvoke(
                      DispatcherPriority.Background,
                      new Action(() => {
                          if(t_keepColor.Enabled)
                              verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background = MainWindow._MainWindow.checkBox_tajweed.IsChecked.Value ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });
                      }));
                };
                t_keepColor.Start();
            }

            waveOut.PlaybackStopped += (sender, e) =>
            {
                LastAudioPlayedDuration = Mp3Reader.TotalTime.TotalMilliseconds;

                t_keepColor.Stop();

                if (verseWords != null)
                    verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });

                waveOut.Dispose();

                PlayNextAudioIfNeeded();

            };
        }

        internal static async Task PlayAudioFromUrl(string url, List<TextBlock> verseWords)
        {
            using (var mf = new MediaFoundationReader(url))
            using (var wo = new WasapiOut())
            {
                wao = wo;
                wo.PlaybackStopped += (sender, e) =>
                {
                    
                    LastAudioPlayedDuration = mf.TotalTime.TotalMilliseconds; 

                    wo.Dispose();
                    mf.Dispose();

                    if (verseWords != null)
                        verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });

                    PlayNextAudioIfNeeded();
                };

                wo.Init(mf);
                wo.Play();
                LastColoredTextBlock = verseWords;

                if (verseWords != null)
                    verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background = MainWindow._MainWindow.checkBox_tajweed.IsChecked.Value ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });

                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    if (verseWords != null)
                        verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background = MainWindow._MainWindow.checkBox_tajweed.IsChecked.Value ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });
                    await Task.Delay(10);
                }
            }
        }

        private static void PlayNextAudioIfNeeded()
        {
            if (((MainWindow._MainWindow.checkbox_LectureAutomatique.IsChecked == true && !ForcedStop && UserControl_QuranReader.TogglePlayPauseAudio && MainWindow._MainWindow.checkbox_repeter_lecture.IsChecked.Value))
            || (MainWindow._MainWindow.checkbox_LectureAutomatique.IsChecked == true && !ForcedStop && (UserControl_QuranReader.TogglePlayPauseAudio)))
            {
                if(!IsAnyAudioPlaying() )
                    UserControl_QuranReader.PlayNext(null, null);
            }
            else
            {
                ForcedStop = false;
            }

            // pas de lecture automatique est audio finit = pause
            if(!MainWindow._MainWindow.checkbox_LectureAutomatique.IsChecked.Value && UserControl_QuranReader. TogglePlayPauseAudio && !IsAnyAudioPlaying())
            {
                UserControl_QuranReader.PlayPause(null, null);
            }
        }

        private static bool ForcedStop = false;

        /// <summary>
        /// Pause tout les audios actuellement en cours
        /// </summary>
        internal static void PauseAllPlayingAudio(bool? force = null)
        {
            ForcedStop =!MainWindow._MainWindow.checkbox_LectureAutomatique.IsChecked.Value;
            if (force != null)
                ForcedStop = force.Value;

            woE.Dispose();
            wo.Dispose();
            wao.Dispose();

            woE.Stop();
            wo.Stop();
            wao.Stop();
        }

        internal static bool IsAnyAudioPlaying()
        {
            return wo.PlaybackState == PlaybackState.Playing || woE.PlaybackState == PlaybackState.Playing || wao.PlaybackState == PlaybackState.Playing;
        }
    }
}
