using Microsoft.VisualBasic.Devices;
using NAudio.Gui;
using NAudio.SoundFont;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.AxHost;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
using Timer = System.Timers.Timer;

namespace House_of_Quran
{
    /// <summary>
    /// Logique d'interaction pour UserControl_QuranReader.xaml
    /// </summary>
    public partial class UserControl_QuranReader : UserControl
    {
        internal int ActualPlayingTextBlockPos = -1;
        internal static Action<object, RoutedEventArgs>? PlayNextLectureStep;
        internal static Action<object, RoutedEventArgs>? PlayNextMemorisationStep;
        internal static Action<object, RoutedEventArgs>? PlayPause;
        private DoubleAnimation Animation = new DoubleAnimation();

        public UserControl_QuranReader()
        {
            InitializeComponent();

            textBlock_textSize.Text = Properties.Settings.Default.DerniereTaille.ToString();
            textBlock_espacement.Text = Properties.Settings.Default.DernierEspacement.ToString();

            PlayNextLectureStep = Button_Right_Click;
            PlayPause = Button_PlayPause_Click;
            PlayNextMemorisationStep = Button_Right_Click;

            T_Checker.Elapsed += (sender, e) =>
            {
                i = 0;
            };
            T_Checker.Start();
        }

        public void AfficherSourate(int id)
        {
            WrapPanel_QuranText.Children.Clear();

            // Set sourate Header
            txtBlock_SourateNomArabe.Text = MainWindow.Quran![id].Name.Trim();
            txtBlock_SourateNombreVerset.Text = MainWindow.Quran![id].Ayahs.Count + " versets";
            txtBlock_typeSourate.Text = MainWindow.Quran![id].RevelationType == "Meccan" ? "Mecquoise" : "Médinoise";

            // Affiche les versets
            foreach (Ayah verset in MainWindow.Quran![id].Ayahs)
            {
                int toSub = 0;
                int addToAudio = 0;
                int max = verset.Text.Split(' ').Length;
                for (int i = 0; i < max; i++)
                {
                    string mot = MainWindow.Quran![id].Ayahs[verset.NumberInSurah - 1].Text.Split(' ')[i];

                    // Lorsque l'on a ya ayouha on le met en un mot
                    // يَٰٓأَيُّهَا
                    // يَا أَيُّهَا
                    if (MainWindow.Quran![id].Ayahs[verset.NumberInSurah - 1].Text.Split(' ').Length - 1 > i + 1)
                        if(mot == "يَا" && MainWindow.Quran![id].Ayahs[verset.NumberInSurah - 1].Text.Split(' ')[i + 1] == "أَيُّهَا")
                        {
                            mot = "يَٰٓأَيُّهَا";
                            toSub++;
                            i++;
                        }

                    TextBlock textBlock_mot = new TextBlock();
                    textBlock_mot.FontSize = Convert.ToInt16(textBlock_textSize.Text);
                    textBlock_mot.Cursor = Cursors.Hand;              

                    // Set le mot
                    if (i < verset.Text.Split(' ').Length - 1) // on ne met pas d'espacement avec un numéro de verset
                        mot = mot.Trim() + new String(' ', Convert.ToInt16(textBlock_espacement.Text));
                    else
                        mot = mot.Trim() + new String(' ', 2);

                    textBlock_mot.Inlines.Add(new Run() { Tag = mot, Text =Properties.Settings.Default.Tajweed ? String.Empty : mot });

                    string url_tajweed = "https://static.qurancdn.com/images/w/rq-color/" + (id + 1) + "/" + verset.NumberInSurah + "/" + ((i + 1) - toSub) + ".png";
                    textBlock_mot.DataContext = url_tajweed;

                    textBlock_mot.Inlines.Add(new InlineUIContainer() 
                    { 
                        Child = new Image()
                        {
                             Width = Utilities.RemoveDiacritics(mot).Length * 10,
                             Height = 80,
                             Stretch = Stretch.Uniform,
                             Visibility =Properties.Settings.Default.Tajweed ? Visibility.Visible : Visibility.Collapsed
                        }
                    });

                    // Event
                    SetUpBasicTextBlockWordEvent(textBlock_mot, Brushes.Tomato);
                    textBlock_mot.Tag = (id + 1).ToString().PadLeft(3, '0') + verset.NumberInSurah.ToString().PadLeft(3, '0') + ((i - toSub )+ 1 + addToAudio).ToString().PadLeft(3, '0');
                    textBlock_mot.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(PlayWordAudio);

                    WrapPanel_QuranText.Children.Add(textBlock_mot);

                    if (mot.Contains("ۘ") || mot.Contains("ۖ") || mot.Contains("ۗ") || mot.Contains("ۙ") || mot.Contains("ۚ") || mot.Contains("ۛ") || mot.Contains("ۜ"))
                    {
                        // C'est un signe de prononciation, l'audio du verset +1
                        addToAudio++;
                    }
                }

                TextBlock textBlock_finVerset = new TextBlock();
                textBlock_finVerset.Text = "﴿" + Utilities.ConvertNumeralsToArabic(verset.NumberInSurah.ToString()) + "﴾    " + new String(' ', Convert.ToInt16(textBlock_espacement.Text));
                textBlock_finVerset.FontSize = Convert.ToInt16(textBlock_textSize.Text);
                textBlock_finVerset.Cursor = Cursors.Hand;
                textBlock_finVerset.ToolTip = verset.NumberInSurah;
                SetUpBasicTextBlockWordEvent(textBlock_finVerset, Brushes.Green);

                // Event
                textBlock_finVerset.Tag = (id + 1).ToString().PadLeft(3, '0') + verset.NumberInSurah.ToString().PadLeft(3, '0');
                textBlock_finVerset.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(PlayVerseAudio);

                // les numéros toujours vont s'afficher avec la police me_quran
                if (!MainWindow.CurrentFont!.ToString().Contains("ScheherazadeRegOT") && !MainWindow.CurrentFont.ToString().Contains("Othmani"))
                    textBlock_finVerset.FontFamily = (FontFamily)FindResource("Me_Quran");

                WrapPanel_QuranText.Children.Add(textBlock_finVerset);
            }

            ActualPlayingTextBlockPos = -1;
            this.Tag = id;

            label_bismillah.FontSize = Convert.ToInt16(textBlock_textSize.Text) / 1.5;
            label_bismillah.Visibility = id == 0 ? Visibility.Collapsed : Visibility.Visible;

            // pour éviter que la méthode soit appelé 2 fois :
            string? t = Properties.Settings.Default.FromWhereToWhereForEachSurah.Cast<string>().ToList().FirstOrDefault(x => x.Split(',')[0] == Tag.ToString());
            if (t == default)
            {
                CreateMemorisationStep();
            }

            // tajweed
            if (Properties.Settings.Default.Tajweed)
                ApplyTajweed(true);
        }


        private  async void PlayWordAudio(object sender, MouseButtonEventArgs e)
        {
            TextBlock? textBlock = (sender is TextBlock) ? (sender as TextBlock) : ((TextBlock)WrapPanel_QuranText.Children[(sender as int[])![0]]);

            if (Properties.Settings.Default.ModeLecture)
            {
                if (!TogglePlayPauseAudio)
                {
                    TogglePlayPauseAudio = true;
                    button_playpause.Foreground = Brushes.DarkGreen;
                    LastWasAudio = true;
                }

                ActualPlayingTextBlockPos = WrapPanel_QuranText.Children.IndexOf(textBlock);
            }

            string? audio = textBlock!.Tag.ToString(); // 001001001
            string file = @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\wbw\" + audio!.Remove(0, 3).Insert(3, "_") + ".mp3";

            // ce n'est pas un mot
            if (!(textBlock.Inlines.ElementAt(0) as Run)!.Text.Contains("۩") && !(textBlock.Inlines.ElementAt(0) as Run)!.Text.Contains("۞"))
            {
                // pause tout les audios actuellement en cours
                AudioUtilities.PauseAllPlayingAudio();
                AudioUtilities.LastColoredTextBlock.ForEach(x => { x.Foreground = AudioUtilities.COLOR_AYAH; x.Background = Brushes.Transparent; });

                // Si on est dans le mode mémorisation et que pourtant c'est l'utilisateur qui a voulu play l'audio on change l'étape actuelle 
                if(e != null && !Properties.Settings.Default.ModeLecture)
                {
                    int t = WrapPanel_QuranText.Children.IndexOf(textBlock);
                    ActualMemorisationStep = MemorisationSteps.FindIndex(x => x.PosInWrapPanel[0] ==t );
                }

                // Set les textblock avec qui l'effet doit être présent
                var txtBlockPlayEffect = new List<TextBlock>();

                if (sender is TextBlock) // mode lecture
                    txtBlockPlayEffect.Add(textBlock!); 
                else if(sender is int[]) // mode memorisation
                {
                    foreach (int i in (sender as int[])!)
                        txtBlockPlayEffect.Add((TextBlock)WrapPanel_QuranText.Children[i]);
                }

                // Audio téléchargé? 
                if (File.Exists(file))
                {
                    if((sender is int[]))
                    {
                        if((sender as int[])!.Length > 1)
                        {
                            // play multiple
                            List<string> files = new List<string>();

                            for (int pos = 0; pos < (sender as int[])!.Length; pos++)
                            {
                                audio = ((TextBlock)WrapPanel_QuranText.Children[(sender as int[])![pos]]).Tag.ToString();
                                files.Add( @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\wbw\" + audio!.Remove(0, 3).Insert(3, "_") + ".mp3");
                            }

                            AudioUtilities.PlayMp3FromLocalFile(files[0], txtBlockPlayEffect, files);
                        }
                        else
                            // Play depuis fichier
                            AudioUtilities.PlayMp3FromLocalFile(file, txtBlockPlayEffect);
                    }
                    else
                        // Play depuis fichier
                        AudioUtilities.PlayMp3FromLocalFile(file, txtBlockPlayEffect);

                }
                else
                {
                    if (MainWindow.HaveInternet)
                    {
                        if ((sender is int[]))
                        {
                            if ((sender as int[])!.Length > 1)
                            {
                                // play multiple
                                List<string> urls = new List<string>();

                                for (int pos = 0; pos < (sender as int[])!.Length; pos++)
                                {
                                    audio = ((TextBlock)WrapPanel_QuranText.Children[(sender as int[])![pos]]).Tag.ToString();
                                    urls.Add("https://audio.qurancdn.com/wbw/" + audio!.Insert(3, "_").Insert(7, "_") + ".mp3");
                                }

                                await AudioUtilities.PlayAudioFromUrl(urls[0], txtBlockPlayEffect, urls);
                            }
                            else
                            {
                                // Play depuis un lien
                                var url = "https://audio.qurancdn.com/wbw/" + audio.Insert(3, "_").Insert(7, "_") + ".mp3";
                                await AudioUtilities.PlayAudioFromUrl(url, txtBlockPlayEffect);
                            }
                        }
                        else
                        {
                            // Play depuis un lien
                            var url = "https://audio.qurancdn.com/wbw/" + audio.Insert(3, "_").Insert(7, "_") + ".mp3";
                            await AudioUtilities.PlayAudioFromUrl(url, txtBlockPlayEffect);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Aucune connexion internet, pensez à télécharger ou à finir le téléchargement de cette sourate avec ce récitateur.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
        }

        private async void PlayVerseAudio(object sender, MouseButtonEventArgs e)
        {
            TextBlock? textBlock = (sender is TextBlock) ? (sender as TextBlock) : ((TextBlock)WrapPanel_QuranText.Children[(sender as int[])![0]]);
            if (Properties.Settings.Default.ModeLecture)
            {
                if (!TogglePlayPauseAudio)
                {
                    TogglePlayPauseAudio = true;
                    button_playpause.Foreground = Brushes.DarkGreen;
                    LastWasAudio = true;
                }

                ActualPlayingTextBlockPos = WrapPanel_QuranText.Children.IndexOf(textBlock);
            }

            string? audio = textBlock!.Tag.ToString(); // 001001
            int recitateurIndex = MainWindow._MainWindow!.comboBox_Recitateur.SelectedIndex;
            string extensionRecitateur = MainWindow._MainWindow!.Recitateurs[recitateurIndex].Extension; // .ogg ou .mp3

            AudioUtilities.PauseAllPlayingAudio();
            AudioUtilities.LastColoredTextBlock.ForEach(x => { x.Foreground = AudioUtilities.COLOR_AYAH; x.Background = Brushes.Transparent; });

            string file = @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\verse\" + recitateurIndex + "-" + audio!.Remove(0, 3) + extensionRecitateur;

            // Récupère tous les TextBlock du verset en question
            List<TextBlock> verseWords = new List<TextBlock>();
            for (int i = WrapPanel_QuranText.Children.IndexOf(sender as TextBlock); i >= 0; i--)
            {
                // la deuxieme condition est là sinon ça s'arrête à la première textBlock qui est un chiffre, celui du verset à jouer
                if (((TextBlock)WrapPanel_QuranText.Children[i]).Text.Contains("﴾") && i != WrapPanel_QuranText.Children.IndexOf(sender as TextBlock)) 
                {
                    break;
                }

                verseWords.Add((TextBlock)WrapPanel_QuranText.Children[i]);
            }

            // Audio téléchargé?
            if (File.Exists(file))
            {
                // Play depuis fichier
                if(extensionRecitateur == ".mp3")
                     AudioUtilities.PlayMp3FromLocalFile(file, verseWords);
                //else // ogg
                //{
                //    await AudioUtilities.PlayOggFromLocalFile(file, verseWords);
                //}
            }
            else
            {
                if (MainWindow.HaveInternet)
                {
                    // Play depuis internet
                    string lien = MainWindow._MainWindow!.Recitateurs[recitateurIndex].Lien + audio + extensionRecitateur;

                    await AudioUtilities.PlayAudioFromUrl(lien, verseWords);
                }
                else
                    MessageBox.Show("Aucune connexion internet, pensez à télécharger cette sourate avec ce récitateur.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        
        private void SetUpBasicTextBlockWordEvent(TextBlock textBlock, Brush mouseHoverColor)
        {         
            textBlock.MouseEnter += (sender, e) =>
            {
                if(textBlock.Foreground != AudioUtilities.COLOR_AYAH_BEING_PLAYED)
                    textBlock.Foreground = mouseHoverColor;
            };

            textBlock.MouseLeave += (sender, e) =>
            {
                if (textBlock.Foreground != AudioUtilities.COLOR_AYAH_BEING_PLAYED)
                    textBlock.Foreground = Brushes.Black;
            };
        }

        /// <summary>
        /// Change la taille du texte
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_SizeChanger_Click(object sender, RoutedEventArgs e)
        {
            int currentSize = Convert.ToInt16(textBlock_textSize.Text);
            if(sender!=null)
                currentSize = ApplyNewValue(((Button)sender).Content.ToString()!, currentSize);

            textBlock_textSize.Text = currentSize.ToString();

            foreach (TextBlock textBlock_mot in WrapPanel_QuranText.Children)
            {
                if(Properties.Settings.Default.Tajweed && !String.IsNullOrEmpty(textBlock_mot.Text))
                    textBlock_mot.FontSize = (Convert.ToInt16(textBlock_textSize.Text) * 2.2) / 2;
                else
                    textBlock_mot.FontSize = Convert.ToInt16(textBlock_textSize.Text);

                try
                {
                    // la size est dans la width du tajweed aussi
                    int i = Utilities.RemoveDiacritics(((Run)textBlock_mot.Inlines.ElementAt(0)).Tag.ToString()!).Length;
                    ((Image)((InlineUIContainer)textBlock_mot.Inlines.ElementAt(1)).Child).Width = (i * Convert.ToInt16(textBlock_textSize.Text)) / 2;
                    ((Image)((InlineUIContainer)textBlock_mot.Inlines.ElementAt(1)).Child).Height = (100 + (textBlock_textSize.Text.Length > 1 ? Convert.ToInt16(textBlock_textSize.Text[0])* 2 : 0.5)) / 2;
                }
                catch { }
                
            }

            label_bismillah.FontSize = Convert.ToInt16(currentSize / 1.5);

            Properties.Settings.Default.DerniereTaille = currentSize;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Change l'espacement entre les mots
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_EspacementChanger_Click(object sender, RoutedEventArgs e)
        {
            int currentSize = Convert.ToInt16(textBlock_espacement.Text);
            if (sender != null)
                currentSize = ApplyNewValue(((Button)sender).Content.ToString()!, currentSize);

            textBlock_espacement.Text = currentSize.ToString();

            for (int i = 0; i < WrapPanel_QuranText.Children.Count - 1; i++)
            {
                TextBlock textBlock_mot = (TextBlock)WrapPanel_QuranText.Children[i];

                if (!((TextBlock)WrapPanel_QuranText.Children[i]).Text.Contains("﴾"))
                {
                    try
                    {
                        ((Run)textBlock_mot.Inlines.ElementAt(0)).Text = textBlock_mot.Text.Trim() + new String(' ', Convert.ToInt16(textBlock_espacement.Text));
                        ((Image)((InlineUIContainer)textBlock_mot.Inlines.ElementAt(1)).Child).Margin = new Thickness(Convert.ToInt16(textBlock_espacement.Text), 0, Convert.ToInt16(textBlock_espacement.Text), 0);
                    }
                    catch { }
                        
                }
                else
                {
                    if(!Properties.Settings.Default.Tajweed)
                        textBlock_mot.Text = textBlock_mot.Text.Trim() + new String(' ', 2);
                }
            }

            Properties.Settings.Default.DernierEspacement = currentSize;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Applique la nouvelle valeur en fonction de s'il on appuie sur + ou sur -
        /// </summary>
        /// <param name="sign">Signe (+/-)</param>
        /// <param name="currentSize">La valeur actuelle</param>
        /// <returns>La nouvelle valeur</returns>
        private static int ApplyNewValue(string sign, int currentValue)
        {
            if (sign == "+")
                currentValue++;
            else
                currentValue--;
            if (currentValue < 1)
                currentValue = 1;
            else if (currentValue > 255)
                currentValue = 255;
            return currentValue;
        }

        private void textBlock_textSize_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                Button_SizeChanger_Click(button_fontSizeUp, null!);
            }
            else
            {
                Button_SizeChanger_Click(button_fontSizeDown, null!);
            }
        }
        
        private void textBlock_espacement_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                Button_EspacementChanger_Click(button_espacementPlus, null!);
            }
            else
            {
                Button_EspacementChanger_Click(button_espacementMoins, null!);
            }
        }

        /// <summary>
        /// Change la police d'écriture des nombres pour seulement certaines qui les prends en charge
        /// </summary>
        /// <param name="newOne"></param>
        internal void CheckFontOfNumber(FontFamily newOne)
        {
            foreach (TextBlock textBlock_mot in WrapPanel_QuranText.Children)
            {
                if(textBlock_mot.Text.Contains("﴾"))
                    if (!newOne.ToString().Contains("ScheherazadeRegOT")  && !newOne.ToString().Contains("Othmani"))
                        textBlock_mot.FontFamily = (FontFamily)FindResource("Me_Quran");
                    else
                        textBlock_mot.FontFamily = newOne;
            }

        }

        private void Border_AudioControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Border_Controles.Opacity = 1;
        }

        private void Border_AudioControl_MouseLeave(object sender, MouseEventArgs e)
        {
            House_of_Quran.Animation.HideAnimation(Border_Controles);
        }

        /// <summary>
        /// Affiche uniquement les versets de debut à fin
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        internal void BorneChanged(int? debut, int? fin)
        {
            foreach(TextBlock txtBlock in WrapPanel_QuranText.Children)
            {
                int vNumber = Convert.ToInt16(txtBlock.Tag.ToString()!.Substring(3, 3));
                if (vNumber < debut || vNumber > fin)
                    txtBlock.Visibility = Visibility.Collapsed;
                else
                    txtBlock.Visibility = Visibility.Visible;
            }

            CreateMemorisationStep();
        }

        internal static bool TogglePlayPauseAudio = false;
        private List<Step> MemorisationSteps = new List<Step>();
        internal static int ActualMemorisationStep = 0;

        /// <summary>
        /// Play/Pause audio
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_PlayPause_Click(object sender, RoutedEventArgs e)
        {
            // ToogglePlayPauseAudio == false : pause
            // ToogglePlayPauseAudio == true : play
            TogglePlayPauseAudio = !TogglePlayPauseAudio;

            if (TogglePlayPauseAudio)
            {
                if(Properties.Settings.Default.ModeLecture)
                {
                    LastWasAudio = true;
                    GetNextTextBlockIndexToPlay(1);
                    PlayCurrentIndex();
                    button_playpause.Foreground = Brushes.DarkGreen;
                }
                else // Mode mémorisation
                {
                    if(MemorisationSteps.Any())
                    {
                        // continue la mémorisation
                        button_playpause.Foreground = Brushes.DarkGreen;
                        PlayCurrentMemorisationStep(MemorisationSteps[ActualMemorisationStep]);
                    }
                    else
                    {
                        // Commence la mémorisation
                        button_playpause.Foreground = Brushes.DarkGreen;
                        PlayCurrentMemorisationStep(MemorisationSteps[ActualMemorisationStep]);
                    }
                }
            }
            else
            {
                StopAnimation();
                button_playpause.Foreground = Brushes.Red;
                AudioUtilities.PauseAllPlayingAudio();
                AudioUtilities.LastColoredTextBlock.ForEach(x => { x.Foreground = AudioUtilities.COLOR_AYAH; x.Background = Brushes.Transparent; });
            }

        }

        private List<string[]> urls_files_times = new List<string[]>(); // pour calculer le temps

        /// <summary>
        /// Créé les étapes de mémorisation
        /// </summary>
        internal void CreateMemorisationStep()
        {
            bool finSourate = false;
            ActualPlayingTextBlockPos = 0;
            ActualMemorisationStep = 0;
            urls_files_times = new List<string[]>();
            MemorisationSteps.Clear();

            TimeSpan temps_total = new TimeSpan(0); // temps total que la mémorisation va prendre

            // Lis 2x le premier mot
            // premier index affiché
            int i = 0;
            foreach (TextBlock txtBlock in WrapPanel_QuranText.Children)
                if (txtBlock.Visibility == Visibility.Collapsed)
                    i++;
                else
                    break;
            ActualPlayingTextBlockPos = i - 1;

            GetNextTextBlockIndexToPlay(1, true);

            string tag = ((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Tag.ToString()!;

            MemorisationSteps.Add(new Step(StepType.LECTURE_MOT, new int[1] { ActualPlayingTextBlockPos }));

            MemorisationSteps.Add(new Step(StepType.REPETITION, new int[1] { ActualPlayingTextBlockPos }));

            MemorisationSteps.Add(new Step(StepType.LECTURE_MOT, new int[1] { ActualPlayingTextBlockPos }));

            // Void du premier mot
            MemorisationSteps.Add(new Step(StepType.VOID, new int[1] { ActualPlayingTextBlockPos }));

            AudioUtilities.TempsAudio("https://audio.qurancdn.com/wbw/" + tag.Insert(3, "_").Insert(7, "_") + ".mp3", @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\wbw\" + tag.Remove(0, 3).Insert(3, "_") + ".mp3", 4, true);

            bool premierVerset = true;
            List<string[]> allUrlAndSurah = new List<string[]>();
            while (!finSourate)
            {
                // 1* mot d'après, 1* répétition mot d'après, 1x mot -1 et 0, 1* répétion mot -1 et 0, une fois void mot -1 et 0
                int actualWord = ActualPlayingTextBlockPos;
                int actualWordMoins2 = -1;
                try
                {
                    actualWordMoins2 = ((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos - 1]).Text.Contains("﴾") ? -1 : ActualPlayingTextBlockPos - 1;
                }
                catch { } // out of range

                string tag2 = ((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Tag.ToString()!;

                GetNextTextBlockIndexToPlay(1, true);

                tag = ((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Tag.ToString()!;

                // si c'est un mot
                if (!((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Text.Contains("﴾"))
                {
                    // si c'est le PREMIER MOT d'un nouveau verset
                    if (((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos - 1]).Text.Contains("﴾"))
                    {
                        // Lis 2x le premier mot
                        MemorisationSteps.Add(new Step(StepType.LECTURE_MOT, new int[1] { ActualPlayingTextBlockPos }));
                        MemorisationSteps.Add(new Step(StepType.REPETITION, new int[1] { ActualPlayingTextBlockPos }));
                        MemorisationSteps.Add(new Step(StepType.LECTURE_MOT, new int[1] { ActualPlayingTextBlockPos }));
                        // Void du premier mot
                        MemorisationSteps.Add(new Step(StepType.VOID, new int[1] { ActualPlayingTextBlockPos }));

                        AudioUtilities.TempsAudio("https://audio.qurancdn.com/wbw/" + tag.Insert(3, "_").Insert(7, "_") + ".mp3", @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\wbw\" + tag.Remove(0, 3).Insert(3, "_") + ".mp3", 4);
                    }
                    else
                    {
                        // lecture
                        MemorisationSteps.Add(new Step(StepType.LECTURE_MOT, new int[1] { ActualPlayingTextBlockPos }));
                        // repetition
                        MemorisationSteps.Add(new Step(StepType.REPETITION, new int[1] { ActualPlayingTextBlockPos }));
                        // 1x lecture mot -1 et 0
                        MemorisationSteps.Add(new Step(StepType.LECTURE_MOT, new int[2] { actualWord, ActualPlayingTextBlockPos }));
                        // 1x repetition mot -1 et 0
                        MemorisationSteps.Add(new Step(StepType.REPETITION, new int[2] { actualWord, ActualPlayingTextBlockPos }));
                        // 1x lecture mot -1 et 0
                        MemorisationSteps.Add(new Step(StepType.LECTURE_MOT, new int[2] { actualWord, ActualPlayingTextBlockPos }));
                        // 1x void mot -1 et 0
                        MemorisationSteps.Add(new Step(StepType.VOID, new int[2] { actualWord, ActualPlayingTextBlockPos }));

                        // 1x lecture mot -2 -1 0 & 1x répétion
                        if(actualWordMoins2 != -1)
                        {
                            string tag3 = ((TextBlock)WrapPanel_QuranText.Children[actualWord - 1]).Tag.ToString()!;
                            MemorisationSteps.Add(new Step(StepType.LECTURE_MOT, new int[3] { actualWordMoins2, actualWord, ActualPlayingTextBlockPos })) ;
                            MemorisationSteps.Add(new Step(StepType.REPETITION, new int[3] { actualWordMoins2, actualWord, ActualPlayingTextBlockPos }));
                            AudioUtilities.TempsAudio("https://audio.qurancdn.com/wbw/" + tag2.Insert(3, "_").Insert(7, "_") + ".mp3", @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\wbw\" + tag2.Remove(0, 3).Insert(3, "_") + ".mp3",2);
                            AudioUtilities.TempsAudio("https://audio.qurancdn.com/wbw/" + tag.Insert(3, "_").Insert(7, "_") + ".mp3", @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\wbw\" + tag.Remove(0, 3).Insert(3, "_") + ".mp3",2);
                            AudioUtilities.TempsAudio("https://audio.qurancdn.com/wbw/" + tag3.Insert(3, "_").Insert(7, "_") + ".mp3", @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\wbw\" + tag3.Remove(0, 3).Insert(3, "_") + ".mp3",2);

                        }

                        AudioUtilities.TempsAudio("https://audio.qurancdn.com/wbw/" + tag2.Insert(3, "_").Insert(7, "_") + ".mp3", @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\wbw\" + tag2.Remove(0, 3).Insert(3, "_") + ".mp3", 4);
                        AudioUtilities.TempsAudio("https://audio.qurancdn.com/wbw/" + tag.Insert(3, "_").Insert(7, "_") + ".mp3", @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\wbw\" + tag.Remove(0, 3).Insert(3, "_") + ".mp3", 6);
                    }
                }
                else
                {
                    // verset donc on l'écoute 1*, on répète 1x, on ré-écoute puis void
                    // lecture 1x
                    MemorisationSteps.Add(new Step(StepType.LECTURE_VERSET, new int[1] { ActualPlayingTextBlockPos }));
                    // répète 1x
                    MemorisationSteps.Add(new Step(StepType.REPETITION, new int[1] { ActualPlayingTextBlockPos }));
                    // lecture 1x
                    MemorisationSteps.Add(new Step(StepType.LECTURE_VERSET, new int[1] { ActualPlayingTextBlockPos }));
                    // void 1x
                    MemorisationSteps.Add(new Step(StepType.VOID, new int[1] { ActualPlayingTextBlockPos }));

                    allUrlAndSurah.Add(new string[] { ActualPlayingTextBlockPos.ToString(), "https://audio.qurancdn.com/wbw/" + tag.Insert(3, "_").Insert(7, "_") + ".mp3", @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\wbw\" + tag.Remove(0, 3).Insert(3, "_") + ".mp3" });
                    // si ce n'est pas le premier verset on répète tous les versets d'avant jusqu'à celui là
                    if (!premierVerset)
                    {
                        for (int v = 0;v < allUrlAndSurah.Count; v++)
                        {
                            MemorisationSteps.Add(new Step(StepType.LECTURE_VERSET, new int[1] { Convert.ToInt16(allUrlAndSurah[v][0]) }));
                            MemorisationSteps.Add(new Step(StepType.REPETITION, new int[1] { Convert.ToInt16(allUrlAndSurah[v][0]) }));
                            MemorisationSteps.Add(new Step(StepType.LECTURE_VERSET, new int[1] { Convert.ToInt16(allUrlAndSurah[v][0]) }));
                            MemorisationSteps.Add(new Step(StepType.VOID, new int[1] { Convert.ToInt16(allUrlAndSurah[v][0]) }));
                            AudioUtilities.TempsAudio(allUrlAndSurah[v][1], allUrlAndSurah[v][2], 4);
                        }
                    }
                    else
                        premierVerset = false;

                    AudioUtilities.TempsAudio("https://audio.qurancdn.com/wbw/" + tag.Insert(3, "_").Insert(7, "_") + ".mp3", @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\wbw\" + tag.Remove(0, 3).Insert(3, "_") + ".mp3", 4);
                }

                // est-ce que c'est la fin?
                // todo: ne fonctionne pas
                if (ActualPlayingTextBlockPos == NumberOfVisibleChildren()- 1)
                    finSourate = true;
            }

            ActualPlayingTextBlockPos = -1;
            // Affiche le temps
            MainWindow._MainWindow!.AfficherTempsApprentissage(new TimeSpan(), true);
        }

        internal void StopLecture()
        {
            button_playpause.Foreground = Brushes.Red;
            AudioUtilities.PauseAllPlayingAudio();
            if (TogglePlayPauseAudio)
                TogglePlayPauseAudio = false;
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                Button_PlayPause_Click(this, null!);
            }
            if (e.Key == Key.Left)
                Button_Left_Click(this, null!);
            if (e.Key == Key.Right)
                Button_Right_Click(this, null!);
        }

        private void Button_Left_Click(object sender, RoutedEventArgs e)
        {
            if (TogglePlayPauseAudio == false)
            {
                button_playpause.Foreground = Brushes.DarkGreen;
                TogglePlayPauseAudio = true;
            }

            if (Properties.Settings.Default.ModeLecture)
            {
                // Play mot/sourate d'avant
                int p = ActualPlayingTextBlockPos;
                try
                {
                    GetNextTextBlockIndexToPlay(-1);
                }
                catch { ActualPlayingTextBlockPos = p; if (((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Visibility == Visibility.Collapsed) GetNextTextBlockIndexToPlay(1); }

                if (ActualPlayingTextBlockPos < 0)
                    ActualPlayingTextBlockPos = 0;

                PlayCurrentIndex();
            }
            else
            {
                // on pause d'abord tout
                AudioUtilities.PauseAllPlayingAudio();

                // Next du mode mémorisation
                ActualMemorisationStep--;
                if (ActualMemorisationStep < 0)
                    ActualMemorisationStep = 0;

                StopAnimation();

                PlayCurrentMemorisationStep(MemorisationSteps[ActualMemorisationStep]);
            }
        }

        private void StopAnimation()
        {
            if (T_Repeter != null)
                T_Repeter.Stop();
            progressBar_repeterAnim1.BeginAnimation(ProgressBar.ValueProperty, null!);
            progressBar_repeterAnim2.BeginAnimation(ProgressBar.ValueProperty, null!);
        }

        private void GetNextTextBlockIndexToPlay(int i, bool forceWordByWordAndVerse = false)
        {
            if (MainWindow._MainWindow!.radioButton_recitation_wbwandverse.IsChecked == true || forceWordByWordAndVerse)
                do
                {
                    ActualPlayingTextBlockPos += i;

                } while (((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Visibility == Visibility.Collapsed
                         || ((Run)((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Inlines.ElementAt(0)).Text.Contains("۩")
                         || ((Run)((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Inlines.ElementAt(0)).Text.Contains("۞")); //۞

            else if (MainWindow._MainWindow!.radioButton_recitation_wbw.IsChecked == true)
            {
                do
                {
                    ActualPlayingTextBlockPos += i;

                } while (((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Text.Contains("﴾")
                         || ((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Visibility == Visibility.Collapsed
                         || ((Run)((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Inlines.ElementAt(0)).Text.Contains("۩")
                         || ((Run)((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Inlines.ElementAt(0)).Text.Contains("۞"));
            }
            else if (MainWindow._MainWindow!.radioButton_recitation_verse.IsChecked == true)
            {
                try
                {
                    do
                    {
                        ActualPlayingTextBlockPos += i;

                    } while (!((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Text.Contains("﴾")
                             || ((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Visibility == Visibility.Collapsed
                             || ((Run)((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Inlines.ElementAt(0)).Text.Contains("۩")
                             || ((Run)((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Inlines.ElementAt(0)).Text.Contains("۞"));
                }
                catch {
                    // sourate finit 
                    ActualPlayingTextBlockPos = NumberOfVisibleChildren() - 1;
                }
            }
        }

        private void PlayCurrentIndex()
        {
            if (((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]).Text.Contains("﴾"))
                PlayVerseAudio(((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]), null!);
            else
                PlayWordAudio(((TextBlock)WrapPanel_QuranText.Children[ActualPlayingTextBlockPos]), null!);
        }

        private bool LastWasAudio = false;
        System.Timers.Timer T_Repeter = new System.Timers.Timer();
        System.Timers.Timer T_Checker = new System.Timers.Timer(200);
        int i = 0;

        internal void Button_Right_Click(object sender, RoutedEventArgs e)
        {
            i++;
            if (i == 2)
            {
                AudioUtilities.PauseAllPlayingAudio(true);
                return;
            }

            if (TogglePlayPauseAudio == false)
            {
                button_playpause.Foreground = Brushes.DarkGreen;
                TogglePlayPauseAudio = true;
            }

            // Mode lecture
            if (Properties.Settings.Default.ModeLecture)
            {
                StopAnimation();

                // Si on est dans une zone de répétion et qu'un audio vient d'être joué (= on doit laisser un blanc)
                if (Properties.Settings.Default.RepeterLecture && LastWasAudio && (!Properties.Settings.Default.UniquementPourVerset || AudioUtilities.LastColoredTextBlock.Any(x => !String.IsNullOrEmpty(x.Text))))
                {
                    // La dernière chose joué n'était pas un audio mais une zone de répétion
                    LastWasAudio = false;

                    // Si le dernier audio n'avait pas de longueur cela veut dire que c'est le premier lancement, on joue donc un audio normalement
                    if (AudioUtilities.LastAudioPlayedDuration == 0)
                    {
                        Button_Right_Click(this, null!);
                        return;
                    }
                    else
                    {
                        RepeterActualTextBlockPos();
                    }
                }
                else
                {
                    LastWasAudio = true; // On va jouer un audio          

                    if (AudioUtilities.LastAudioPlayedDuration == 0)
                    {
                        Button_Right_Click(this, null!);
                        return;
                    }

                    // Fin de la sourate on s'arrête
                    if ((ActualPlayingTextBlockPos == NumberOfVisibleChildren() - 1 && (Properties.Settings.Default.ChoixRecitation == 1 ||
                        Properties.Settings.Default.ChoixRecitation == 3)) || (ActualPlayingTextBlockPos == NumberOfVisibleChildren() - 2 &&
                        Properties.Settings.Default.ChoixRecitation == 2))
                    {
                        ActualPlayingTextBlockPos = NumberOfVisibleChildren() - 1;
                        button_playpause.Background = Brushes.Transparent;
                        AudioUtilities.PauseAudio();
                        TogglePlayPauseAudio = false;
                        button_playpause.Foreground = Brushes.Red;
                        if (Properties.Settings.Default.RepeterLecture)
                            Border_AudioControl_MouseLeave(this, null!); // pour pas qu'elle soit en opacité 1 vu que progress barre est dedans
                        return;
                    }

                    // récupère la nouvelle position à jouer
                    int p = ActualPlayingTextBlockPos;

                    try
                    {
                        GetNextTextBlockIndexToPlay(1);
                    }
                    catch { ActualPlayingTextBlockPos = p; }

                    // vérifie que la position n'est pas hors des limites
                    if (ActualPlayingTextBlockPos >= WrapPanel_QuranText.Children.Count)
                    {
                        ActualPlayingTextBlockPos = WrapPanel_QuranText.Children.Count - 1;
                    }

                    // Si on est actuellement dans une pose de répétition on l'enlève pour pouvoir jouer l'audio
                    if (T_Repeter.Enabled)
                        T_Repeter.Stop();

                    // Joue l'audio
                    PlayCurrentIndex();
                }
            }
            else
            {
                if (MemorisationSteps.Any())
                {
                    // on pause d'abord tout
                    AudioUtilities.PauseAllPlayingAudio();
                    
                    // Next du mode mémorisation
                    ActualMemorisationStep++;
                    if (ActualMemorisationStep >= MemorisationSteps.Count)
                        ActualMemorisationStep = MemorisationSteps.Count - 1;

                    StopAnimation();


                    PlayCurrentMemorisationStep(MemorisationSteps[ActualMemorisationStep]);
                }
            }
        }


        private int NumberOfVisibleChildren()
        {
            // compte les premieres collapsed puis les visible et enfin les -
            int i = 0;
            bool startWithCollapsed = false;
            foreach (TextBlock txtblock in WrapPanel_QuranText.Children)
            {
                if(txtblock.Visibility == Visibility.Collapsed && i == 0)                
                    startWithCollapsed = true;

                if (startWithCollapsed && txtblock.Visibility == Visibility.Visible)
                    startWithCollapsed = false;

                if (txtblock.Visibility == Visibility.Visible || startWithCollapsed)
                    i++;
            }


            return i;
        }

        private void RepeterActualTextBlockPos(bool hideWords = false)
        {
            // Timer du temps à faire le blanc
            // on ajoute au temps d'origine le % Properties.Settings.Default.TempsRepeter du temps
            double temps_a_ajouter = ((double)(Properties.Settings.Default.ModeLecture ? Properties.Settings.Default.TempsRepeter : Properties.Settings.Default.TempsRepeterMemo) / (double)100) * AudioUtilities.LastAudioPlayedDuration;
            double total_repeter_duration = AudioUtilities.LastAudioPlayedDuration + temps_a_ajouter;

            total_repeter_duration += Properties.Settings.Default.ModeLecture ? Properties.Settings.Default.TempsRepeterSeconde * 1000 : Properties.Settings.Default.TempsRepeterMemoSeconde * 1000;

            T_Repeter = new Timer(total_repeter_duration);

            if(!hideWords)
                AudioUtilities.LastColoredTextBlock.ForEach(x => { x.Foreground = Brushes.Blue; x.Background =Properties.Settings.Default.Tajweed ? Brushes.LightBlue : Brushes.Transparent; });
            else
                AudioUtilities.LastColoredTextBlock.ForEach(x => { x.Foreground = Brushes.Transparent; if(Properties.Settings.Default.Tajweed) ((Image)((InlineUIContainer)x.Inlines.ElementAt(1)).Child).Visibility = Visibility.Hidden; });

            // Animation répéter
            progressBar_repeterAnim1.Maximum = total_repeter_duration;
            progressBar_repeterAnim2.Maximum = total_repeter_duration;
            Border_Controles.Opacity = 1;

            TimeSpan duration = TimeSpan.FromMilliseconds(total_repeter_duration);
            Animation = new DoubleAnimation(total_repeter_duration, duration);
            progressBar_repeterAnim1.BeginAnimation(ProgressBar.ValueProperty, Animation);
            progressBar_repeterAnim2.BeginAnimation(ProgressBar.ValueProperty, Animation);

            // Fait le blanc et joue l'audio suivant lorsque celui ci est terminé
            T_Repeter.Elapsed += (sender, e) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    // value = 0
                    progressBar_repeterAnim1.BeginAnimation(ProgressBar.ValueProperty, null!);
                    progressBar_repeterAnim2.BeginAnimation(ProgressBar.ValueProperty, null!);

                    if (!AudioUtilities.IsAnyAudioPlaying())
                    {
                        AudioUtilities.LastColoredTextBlock.ForEach(x => { x.Foreground = AudioUtilities.COLOR_AYAH; x.Background = Brushes.Transparent; if (Properties.Settings.Default.Tajweed) ((Image)((InlineUIContainer)x.Inlines.ElementAt(1)).Child).Visibility = Visibility.Visible; });

                        T_Repeter.Stop();
                        Button_Right_Click(this, null!);
                    }
                });

            };

            T_Repeter.Start();
        }

        private void PlayCurrentMemorisationStep(Step step)
        {
            switch (step.Type)
            {
                case StepType.LECTURE_MOT:               
                    PlayWordAudio(step.PosInWrapPanel, null!);
                    break;
                case StepType.LECTURE_VERSET:
                    PlayVerseAudio(WrapPanel_QuranText.Children[step.PosInWrapPanel[0]], null!);
                    break;
                case StepType.REPETITION:
                    RepeterActualTextBlockPos(false);
                    break;
                case StepType.VOID:
                    RepeterActualTextBlockPos(true);
                    break;

            }
        }

        internal void ApplyTajweed(bool value)
        {
            foreach(TextBlock txtBlock in WrapPanel_QuranText.Children)
            {
                if (txtBlock.Inlines.Count == 2) // = est un mot
                {
                    try
                    {
                        if (value && MainWindow.HaveInternet)
                        {
                            if (((Image)((InlineUIContainer)txtBlock.Inlines.ElementAt(1)).Child).Source == null)
                            {

                                var bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.UriSource = new Uri(txtBlock.DataContext.ToString()!);
                                bitmapImage.EndInit();
                                ((Image)((InlineUIContainer)txtBlock.Inlines.ElementAt(1)).Child).Source = bitmapImage;

                            }

                            (((InlineUIContainer)txtBlock.Inlines.ElementAt(1)).Child).Visibility = Visibility.Visible;
                            ((Run)txtBlock.Inlines.ElementAt(0)).Text = String.Empty;

                            
                        }
                        else if (value && !MainWindow.HaveInternet)
                        {
                            // on regarde si le tajweed est téléchargé
                            string file = AppDomain.CurrentDomain.BaseDirectory+ @"data\quran\" + MainWindow.Quran![(int)this.Tag].EnglishName + @"\tajweed\" +
                                                   txtBlock.DataContext.ToString()!.Substring(txtBlock.DataContext.ToString()!.LastIndexOf("r/"), txtBlock.DataContext.ToString()!.Length
                                                 - txtBlock.DataContext.ToString()!.LastIndexOf("r/")).Replace("r/", string.Empty).Replace("/", "_");

                            if(File.Exists(file))
                            {
                                var bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.UriSource = new Uri(file);
                                bitmapImage.EndInit();
                                ((Image)((InlineUIContainer)txtBlock.Inlines.ElementAt(1)).Child).Source = bitmapImage;

                                (((InlineUIContainer)txtBlock.Inlines.ElementAt(1)).Child).Visibility = Visibility.Visible;
                                ((Run)txtBlock.Inlines.ElementAt(0)).Text = String.Empty;
                            }
                            else
                            {
                                MessageBox.Show("Aucune connexion internet, pensez à télécharger ou à finir le téléchargement de cette sourate pour pouvoir avoir le tajweed.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                MainWindow._MainWindow!.checkBox_tajweed.IsChecked = false;
                                break;
                            }
                        }
                        else
                        {
                            (((InlineUIContainer)txtBlock.Inlines.ElementAt(1)).Child).Visibility = Visibility.Collapsed;
                            ((Run)txtBlock.Inlines.ElementAt(0)).Text = ((Run)txtBlock.Inlines.ElementAt(0)).Tag.ToString();
                        }
                    }
                    catch
                    {
                    }

                }

            }

            Button_SizeChanger_Click(null!, null!);
            Button_EspacementChanger_Click(null!, null!);
        }
    }
}
