using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.AxHost;

namespace House_of_Quran
{
    /// <summary>
    /// Logique d'interaction pour UserControl_QuranReader.xaml
    /// </summary>
    public partial class UserControl_QuranReader : UserControl
    {
        public UserControl_QuranReader()
        {
            InitializeComponent();

            textBlock_textSize.Text = Properties.Settings.Default.DerniereTaille.ToString();
            textBlock_espacement.Text = Properties.Settings.Default.DernierEspacement.ToString();
        }

        public void AfficherSourate(int id)
        {
            WrapPanel_QuranText.Children.Clear();

            // Set sourate Header
            txtBlock_SourateNomArabe.Text = "  سورة" + MainWindow.Quran[id].Name;
            txtBlock_SourateNombreVerset.Text = MainWindow.Quran[id].Ayahs.Count + " versets";
            txtBlock_typeSourate.Text = MainWindow.Quran[id].RevelationType == "Meccan" ? "Mecquoise" : "Médinoise";

            // Affiche les versets
            foreach (Ayah verset in MainWindow.Quran[id].Ayahs)
            {
                for (int i = 0; i < verset.Text.Split(' ').Length; i++)
                {
                    string mot = MainWindow.Quran[id].Ayahs[verset.NumberInSurah - 1].Text.Split(' ')[i];
                    
                    if (mot.Contains("ۘ") || mot.Contains("ۖ") || mot.Contains("ۗ") || mot.Contains("ۙ") || mot.Contains("ۚ") || mot.Contains("ۛ") || mot.Contains("ۜ"))
                    {
                        // C'est un signe de prononciation, on le met avec le mot d'avant
                        (WrapPanel_QuranText.Children[WrapPanel_QuranText.Children.Count - 1] as TextBlock).Text += mot;
                        continue;
                    }

                    TextBlock textBlock_mot = new TextBlock();
                    textBlock_mot.FontSize = Convert.ToInt16(textBlock_textSize.Text);

                    // Set le mot
                    if (i < verset.Text.Split(' ').Length - 1) // on ne met pas d'espacement avec un numéro de verset
                        textBlock_mot.Text = mot.Trim() + new String(' ', Convert.ToInt16(textBlock_espacement.Text));
                    else
                        textBlock_mot.Text = mot.Trim() + new String(' ', 2);

                    textBlock_mot.Cursor = Cursors.Hand;

                    SetUpBasicTextBlockWordEvent(textBlock_mot, Brushes.Tomato);

                    // Event
                    textBlock_mot.Tag = (id + 1).ToString().PadLeft(3, '0') + verset.NumberInSurah.ToString().PadLeft(3, '0') + (i + 1).ToString().PadLeft(3, '0');
                    textBlock_mot.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(PlayWordAudio);

                    WrapPanel_QuranText.Children.Add(textBlock_mot);
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
                if (!MainWindow.CurrentFont.ToString().Contains("ScheherazadeRegOT") && !MainWindow.CurrentFont.ToString().Contains("Othmani"))
                    textBlock_finVerset.FontFamily = (FontFamily)FindResource("Me_Quran");

                WrapPanel_QuranText.Children.Add(textBlock_finVerset);
            }

            this.Tag = id;
        }


        private  async void PlayWordAudio(object sender, MouseButtonEventArgs e)
        {
            string audio = (sender as TextBlock).Tag.ToString(); // 001001001
            string file = @"data\quran\" + MainWindow.Quran[(int)this.Tag].EnglishName + @"\wbw\" + audio.Remove(0, 3).Insert(3, "_") + ".mp3";

            // ce n'est pas un mot
            if (!(sender as TextBlock).Text.Contains("۞") && !(sender as TextBlock).Text.Contains("۩"))
            {

                // pause tout les audios actuellement en cours
                AudioUtilities.PauseAllPlayingAudio();

                // Audio téléchargé? 
                if (File.Exists(file))
                {
                    // Play depuis fichier
                    AudioUtilities.PlayMp3FromLocalFile(file, new List<TextBlock> { (sender as TextBlock) });
                }
                else
                {
                    // Play depuis un lien
                    var url = "https://audio.qurancdn.com/wbw/" + audio.Insert(3, "_").Insert(7, "_") + ".mp3";
                    await AudioUtilities.PlayAudioFromUrl(url, new List<TextBlock> { (sender as TextBlock) });
                }
            }

        }

        private async void PlayVerseAudio(object sender, MouseButtonEventArgs e)
        {
            string audio = (sender as TextBlock).Tag.ToString(); // 001001
            int recitateurIndex = MainWindow._MainWindow.comboBox_Recitateur.SelectedIndex;
            string extensionRecitateur = MainWindow._MainWindow.Recitateurs[recitateurIndex].Extension; // .ogg ou .mp3

            AudioUtilities.PauseAllPlayingAudio();

            string file = @"data\quran\" + MainWindow.Quran[(int)this.Tag].EnglishName + @"\verse\" + recitateurIndex + "-" + audio.Remove(0, 3) + extensionRecitateur;

            // Récupère tous les TextBlock du verset en question
            List<TextBlock> verseWords = new List<TextBlock>();
            for (int i = WrapPanel_QuranText.Children.IndexOf(sender as TextBlock) - 1; i >= 0; i--)
            {
                if ((WrapPanel_QuranText.Children[i] as TextBlock).Text.Contains("﴾"))
                    break;

                verseWords.Add(WrapPanel_QuranText.Children[i] as TextBlock);
            }

            // Audio téléchargé?
            if (File.Exists(file))
            {
                // Play depuis fichier
                if(extensionRecitateur == ".mp3")
                     AudioUtilities.PlayMp3FromLocalFile(file, verseWords);
                else // ogg
                {
                    await AudioUtilities.PlayOggFromLocalFile(file, verseWords);
                }
            }
            else
            {
                // Play depuis internet
                string lien = MainWindow._MainWindow.Recitateurs[recitateurIndex].Lien + audio + extensionRecitateur;

                await AudioUtilities.PlayOggFileFromUrl(lien, verseWords);
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
            currentSize = ApplyNewValue((sender as Button).Content.ToString(), currentSize);

            textBlock_textSize.Text = currentSize.ToString();

            foreach (TextBlock textBlock_mot in WrapPanel_QuranText.Children)
            {
                textBlock_mot.FontSize = Convert.ToInt16(textBlock_textSize.Text);
            }

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
            currentSize = ApplyNewValue((sender as Button).Content.ToString(), currentSize);

            textBlock_espacement.Text = currentSize.ToString();

            for (int i = 0; i < WrapPanel_QuranText.Children.Count - 1; i++)
            {
                TextBlock textBlock_mot = (TextBlock)WrapPanel_QuranText.Children[i];

                if (!(WrapPanel_QuranText.Children[i+1] as TextBlock).Text.Contains("﴾")) // on ne met pas d'espace avant un numéro de verset
                    textBlock_mot.Text = textBlock_mot.Text.Trim() + new String(' ', Convert.ToInt16(textBlock_espacement.Text));
                else
                    textBlock_mot.Text = textBlock_mot.Text.Trim() + new String(' ', 2);
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
                Button_SizeChanger_Click(button_fontSizeUp, null);
            }
            else
            {
                Button_SizeChanger_Click(button_fontSizeDown, null);
            }
        }
        
        private void textBlock_espacement_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                Button_EspacementChanger_Click(button_espacementPlus, null);
            }
            else
            {
                Button_EspacementChanger_Click(button_espacementMoins, null);
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
            Animation.HideAnimation(Border_Controles);
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
                int vNumber = Convert.ToInt16(txtBlock.Tag.ToString().Substring(3, 3));
                if (vNumber < debut || vNumber > fin)
                    txtBlock.Visibility = Visibility.Collapsed;
                else
                    txtBlock.Visibility = Visibility.Visible;
            }
        }
    }
}
