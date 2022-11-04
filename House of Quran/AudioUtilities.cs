using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
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
                    LastAudioPlayedDuration = vorbisStream.TotalTime.TotalMilliseconds;

                    waveOut.PlaybackStopped += (sender, e) =>
                    {
                        if (verseWords != null)
                            verseWords.ForEach(x => { x.Foreground = COLOR_AYAH;x.Background = Brushes.Transparent; }) ;

                        waveOut.Dispose();
                        vorbisStream.Dispose();

                        if (MainWindow._MainWindow.radioButton_modeLecture.IsChecked.Value)
                            PlayNextAudioIfNeeded();
                        else if (UserControl_QuranReader.TogglePlayPauseAudio)
                            PlayNextMemorisationStep();
                    };

                    waveOut.Init(vorbisStream);
                    waveOut.Play();
                    _waveOut = waveOut;
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
                LastAudioPlayedDuration = vorbisStream.TotalTime.TotalMilliseconds;

                waveOut.PlaybackStopped += (sender, e) =>
                {

                    if (verseWords != null)
                        verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });

                    waveOut.Dispose();
                    vorbisStream.Dispose();

                    if (MainWindow._MainWindow.radioButton_modeLecture.IsChecked.Value)
                        PlayNextAudioIfNeeded();
                    else if (UserControl_QuranReader.TogglePlayPauseAudio)
                        PlayNextMemorisationStep();
                };

                waveOut.Init(vorbisStream);
                waveOut.Play();
                _waveOut = waveOut;

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

        private static IWavePlayer _waveOut;


        private static bool PauseEvent = true;

        internal static void PauseAudio()
        {
            _waveOut.Pause();
            ForcedStop = true;
        }

        internal static void PlayAudio()
        {
            try { _waveOut.Play();  } catch { } 
        }

        internal static async void PlayMp3FromLocalFile(string file, List<TextBlock> verseWords = null, List<string> files = null)
        {
            int currentFileIndex = 0;

            Mp3FileReader Mp3Reader = new Mp3FileReader(file);
            var waveOut = new WaveOut();
            waveOut.Init(Mp3Reader);
            waveOut.Play();
            _waveOut = waveOut;
            LastColoredTextBlock = verseWords;

            if (verseWords != null)
                verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background = MainWindow._MainWindow.checkBox_tajweed.IsChecked.Value ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });

            var t_keepColor = new System.Timers.Timer(10);

            if (verseWords != null)
            {
                // anim : keep color
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
            LastAudioPlayedDuration = Mp3Reader.TotalTime.TotalMilliseconds;

            waveOut.PlaybackStopped += (sender, e) =>
            {
                if (files != null)
                {
                    // joue le deuxieme audio
                    currentFileIndex++;
                    Mp3Reader = new Mp3FileReader(files[currentFileIndex]);
                    waveOut = new WaveOut();
                    waveOut.Init(Mp3Reader);
                    waveOut.Play();
                    _waveOut = waveOut;
                    LastAudioPlayedDuration += Mp3Reader.TotalTime.TotalMilliseconds;

                    waveOut.PlaybackStopped += (sender, e) =>
                    {
                        t_keepColor.Stop();

                        if (verseWords != null)
                            verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });

                        waveOut.Dispose();


                        PlayNextMemorisationStep();
                    };
                }
                else
                {
                    t_keepColor.Stop();

                    if (verseWords != null)
                        verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });

                    waveOut.Dispose();

                    if (MainWindow._MainWindow.radioButton_modeLecture.IsChecked.Value)
                        PlayNextAudioIfNeeded();
                    else if (UserControl_QuranReader.TogglePlayPauseAudio)
                        PlayNextMemorisationStep();
                }
            };
        }

        internal static async Task PlayAudioFromUrl(string url, List<TextBlock> verseWords, List<string> urls = null)
        {
            int currentFileIndex = 0;

            var mf = new MediaFoundationReader(url);
            var wo = new WasapiOut();
            {
                _waveOut = wo;
                LastAudioPlayedDuration = mf.TotalTime.TotalMilliseconds;

                wo.PlaybackStopped += (sender, e) =>
                {
                    if (urls != null)
                    {
                        // joue le deuxieme audio
                        currentFileIndex++;
                        mf = new MediaFoundationReader(urls[currentFileIndex]);
                        wo = new WasapiOut();
                        {
                            wo.Init(mf);
                            wo.Play();

                            LastAudioPlayedDuration += mf.TotalTime.TotalMilliseconds;

                            wo.PlaybackStopped += (sender, e) =>
                            {
                                if (verseWords != null)
                                    verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });

                                PlayNextMemorisationStep();
                            };
                        }

                    }
                    else
                    {

                        wo.Dispose();
                        mf.Dispose();

                        if (verseWords != null)
                            verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });

                        if (MainWindow._MainWindow.radioButton_modeLecture.IsChecked.Value)
                            PlayNextAudioIfNeeded();
                        else if(UserControl_QuranReader.TogglePlayPauseAudio)
                            PlayNextMemorisationStep();
                    }
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
                    UserControl_QuranReader.PlayNextLectureStep(null, null);
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

            try
            {
                _waveOut.Dispose();
                _waveOut.Stop();
            }
            catch { }

        }

        internal static bool IsAnyAudioPlaying()
        {
            return _waveOut.PlaybackState == PlaybackState.Playing;
        }

        private static void PlayNextMemorisationStep()
        {
            UserControl_QuranReader.PlayNextMemorisationStep(null, null);
        }
    }
}
