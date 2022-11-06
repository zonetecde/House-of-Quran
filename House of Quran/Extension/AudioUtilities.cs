using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Security.Policy;
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
        internal static List<TextBlock> LastColoredTextBlock = new List<TextBlock>();

        internal static double LastAudioPlayedDuration = 0;

        //internal static async Task PlayOggFileFromUrl(string lien, List<TextBlock> verseWords)
        //{
        //    var req = System.Net.WebRequest.Create(lien);
        //    using (Stream stream = req.GetResponse().GetResponseStream())
        //    {
        //        using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(stream))
        //        using (var waveOut = new NAudio.Wave.WaveOutEvent())
        //        {
        //            LastAudioPlayedDuration = vorbisStream.TotalTime.TotalMilliseconds;

        //            waveOut.PlaybackStopped += (sender, e) =>
        //            {
        //                // Redonne leur couleur d'origine
        //                if (verseWords != null)
        //                    verseWords.ForEach(x => { x.Foreground = COLOR_AYAH;x.Background = Brushes.Transparent; }) ;

        //                waveOut.Dispose();
        //                vorbisStream.Dispose();

        //                if (Properties.Settings.Default.ModeLecture)
        //                    PlayNextAudioIfNeeded();
        //                else if (UserControl_QuranReader.TogglePlayPauseAudio)
        //                    PlayNextMemorisationStep();
        //            };

        //            waveOut.Init(vorbisStream);
        //            waveOut.Play();
        //            _waveOut = waveOut;
        //            LastColoredTextBlock = verseWords;

        //            if (verseWords != null)
        //                verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background =Properties.Settings.Default.Tajweed ? Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });

        //            while (waveOut.PlaybackState == PlaybackState.Playing)
        //            {
        //                if (verseWords != null)
        //                    verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background =Properties.Settings.Default.Tajweed ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });
        //                await Task.Delay(10);
        //            }


        //        }
        //    }
        //}

        //internal static async Task PlayOggFromLocalFile(string file, List<TextBlock> verseWords)
        //{
        //    using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(file))
        //    using (var waveOut = new NAudio.Wave.WaveOutEvent())
        //    {
        //        LastAudioPlayedDuration = vorbisStream.TotalTime.TotalMilliseconds;

        //        waveOut.PlaybackStopped += (sender, e) =>
        //        {

        //            if (verseWords != null)
        //                verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });

        //            waveOut.Dispose();
        //            vorbisStream.Dispose();

        //            if (Properties.Settings.Default.ModeLecture)
        //                PlayNextAudioIfNeeded();
        //            else if (UserControl_QuranReader.TogglePlayPauseAudio)
        //                PlayNextMemorisationStep();
        //        };

        //        waveOut.Init(vorbisStream);
        //        waveOut.Play();
        //        _waveOut = waveOut;

        //        LastColoredTextBlock = verseWords;

        //        if (verseWords != null)
        //            verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background =Properties.Settings.Default.Tajweed ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });

        //        while (waveOut.PlaybackState == PlaybackState.Playing)
        //        {
        //            if (verseWords != null)
        //                verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background =Properties.Settings.Default.Tajweed ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });
        //            await Task.Delay(10);
        //        }
        //    }
        //}

        private static IWavePlayer? _waveOut;

        internal static void PauseAudio()
        {
            _waveOut!.Pause();
            ForcedStop = true;
        }

        internal static void PlayAudio()
        {
            try { _waveOut!.Play();  } catch { } 
        }

        internal static System.Timers.Timer t_keepColor = new System.Timers.Timer();

        internal static void PlayMp3FromLocalFile(string file, List<TextBlock>? verseWords = null, List<string>? files = null)
        {
            int currentFileIndex = 0;

            Mp3FileReader Mp3Reader = new Mp3FileReader(file);
            var waveOut = new WaveOut();
            waveOut.Init(Mp3Reader);
            waveOut.Play();
            _waveOut = waveOut;
            LastColoredTextBlock = verseWords!;

            if (verseWords != null)
                verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background = Properties.Settings.Default.Tajweed ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });

            t_keepColor = new System.Timers.Timer(10);

            if (verseWords != null)
            {
                // anim : keep color
                t_keepColor.Elapsed += (sender, e) =>
                {
                    Application.Current.Dispatcher.BeginInvoke(
                      DispatcherPriority.Background,
                      new Action(() => {
                          if(t_keepColor.Enabled)
                              verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background =Properties.Settings.Default.Tajweed ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });
                      }));
                };
                t_keepColor.Start();
            }
            LastAudioPlayedDuration = Mp3Reader.TotalTime.TotalMilliseconds;

            waveOut.PlaybackStopped += async (sender, e) =>
            {
                if (files != null)
                {
                    // joue le deuxieme audio
                    while (currentFileIndex <= files.Count - 2) // - 2 sinon on peut faire do while ( car currentFileIndex++ à l'intérieur)
                    {
                        currentFileIndex++;

                        Mp3Reader = new Mp3FileReader(files[currentFileIndex]);
                        waveOut = new WaveOut();
                        waveOut.Init(Mp3Reader);
                        waveOut.Play();
                        _waveOut = waveOut;

                        LastAudioPlayedDuration += Mp3Reader.TotalTime.TotalMilliseconds;

                        await Task.Delay((int)Mp3Reader.TotalTime.TotalMilliseconds + 100);

                        waveOut.PlaybackStopped += (sender, e) =>
                        {
                            waveOut.Dispose();
                        };
                        
                    }

                    if (verseWords != null)
                        verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });
                    verseWords = new List<TextBlock>();

                    PlayNextMemorisationStep();
                }
                else
                {
                    t_keepColor.Stop();

                    if (verseWords != null)
                        verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });
                    verseWords = new List<TextBlock>();

                    waveOut.Dispose();

                    if (Properties.Settings.Default.ModeLecture)
                        PlayNextAudioIfNeeded();
                    else if (UserControl_QuranReader.TogglePlayPauseAudio)
                        PlayNextMemorisationStep();
                }
            };
        }

        internal static async Task PlayAudioFromUrl(string url, List<TextBlock> verseWords, List<string>? urls = null)
        {
            int currentFileIndex = 0;

            var mf = new MediaFoundationReader(url);
            var wo = new WasapiOut();
            {
                _waveOut = wo;
                LastAudioPlayedDuration = mf.TotalTime.TotalMilliseconds;

                wo.PlaybackStopped += async (sender, e) =>
                {
                    if (urls != null)
                    {
                        // joue le deuxieme audio
                        while (currentFileIndex <= urls.Count - 2) // - 2 sinon on peut faire do while ( car currentFileIndex++ à l'interieur)
                        {
                            currentFileIndex++;

                            mf = new MediaFoundationReader(urls[currentFileIndex]);
                            wo = new WasapiOut();
                            {
                                wo.Init(mf);
                                wo.Play();

                                LastAudioPlayedDuration += mf.TotalTime.TotalMilliseconds;

                                await Task.Delay((int)mf.TotalTime.TotalMilliseconds);

                                wo.PlaybackStopped += (sender, e) =>
                                {
                                    wo.Dispose();
                                };
                            }
                        }

                        if (verseWords != null)
                            verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });
                        verseWords = new List<TextBlock>();

                        PlayNextMemorisationStep();
                    }
                    else
                    {

                        wo.Dispose();
                        mf.Dispose();

                        if (verseWords != null)
                            verseWords.ForEach(x => { x.Foreground = COLOR_AYAH; x.Background = Brushes.Transparent; });
                        verseWords = new List<TextBlock>();

                        if (Properties.Settings.Default.ModeLecture)
                            PlayNextAudioIfNeeded();
                        else if(UserControl_QuranReader.TogglePlayPauseAudio)
                            PlayNextMemorisationStep();
                    }
                };

                wo.Init(mf);
                wo.Play();
                LastColoredTextBlock = verseWords;

                if (verseWords != null)
                    verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background =Properties.Settings.Default.Tajweed ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });

                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    if (verseWords != null)
                        verseWords.ForEach(x => { x.Foreground = COLOR_AYAH_BEING_PLAYED; x.Background =Properties.Settings.Default.Tajweed ? System.Windows.Media.Brushes.Moccasin : System.Windows.Media.Brushes.Transparent; });
                    await Task.Delay(100);
                }
            }
        }

        private static void PlayNextAudioIfNeeded()
        {
            if (((MainWindow._MainWindow!.checkbox_LectureAutomatique.IsChecked == true && !ForcedStop && UserControl_QuranReader.TogglePlayPauseAudio && MainWindow._MainWindow!.checkbox_repeter_lecture.IsChecked!.Value))
            || (MainWindow._MainWindow!.checkbox_LectureAutomatique.IsChecked == true && !ForcedStop && (UserControl_QuranReader.TogglePlayPauseAudio)))
            {
                if(!IsAnyAudioPlaying() )
                    UserControl_QuranReader.PlayNextLectureStep!(null!, null!);
            }
            else
            {
                ForcedStop = false;
            }

            // pas de lecture automatique est audio finit = pause
            if(!MainWindow._MainWindow!.checkbox_LectureAutomatique.IsChecked!.Value && UserControl_QuranReader. TogglePlayPauseAudio && !IsAnyAudioPlaying())
            {
                UserControl_QuranReader.PlayPause!(null!, null!);
            }
        }

        private static bool ForcedStop = false;

        /// <summary>
        /// Pause tout les audios actuellement en cours
        /// </summary>
        internal static void PauseAllPlayingAudio(bool? force = null)
        {
            t_keepColor.Stop();
            ForcedStop =!Properties.Settings.Default.LectureAutomatique;
            if (force != null)
                ForcedStop = force.Value;

            try
            {
                _waveOut!.Dispose();
                _waveOut.Stop();
            }
            catch { }
        }

        internal static bool IsAnyAudioPlaying()
        {
            return _waveOut!.PlaybackState == PlaybackState.Playing;
        }

        private static void PlayNextMemorisationStep()
        {
            UserControl_QuranReader.PlayNextMemorisationStep!(null!, null!);
        }

        private static List<long> temps = new List<long>();
        private static List<BackgroundWorker> bgWorkers = new List<BackgroundWorker>();

        internal static void TempsAudio(string url, string file, int multiplicateur, bool startNew = false)
        {
            if (startNew)
            {
                bgWorkers.ForEach(x => { x.CancelAsync(); x.Dispose(); }); ;
                bgWorkers.Clear();
                temps.Clear();
            }

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += (sender, e) =>
            {
                long i = 0;
                if (System.IO.File.Exists(file))
                {
                    try
                    {
                        i = (new Mp3FileReader(file).TotalTime.Ticks);
                    }
                    catch { i = 1000; }
                }
                else
                {
                    if (MainWindow.HaveInternet)
                    {
                        try
                        {
                            i = new MediaFoundationReader(url).TotalTime.Ticks;
                        }
                        catch { i = 0; }
                    }
                    else
                    {
                        i = 0;
                    }
                }

                temps.Add(i * multiplicateur);
            };
            bgWorkers.Add(worker);
            worker.RunWorkerAsync();
            worker.RunWorkerCompleted += (sender, e) =>
            {
                try
                {
                    MainWindow._MainWindow!.AfficherTempsApprentissage(new TimeSpan(temps.Sum()));
                }
                catch
                {
                    // collection modified
                }
                worker.Dispose();
            };
        }
    }
}
