using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
                    TextBlock textBlock_mot = new TextBlock();
                    textBlock_mot.FontSize = Convert.ToInt16(textBlock_textSize.Text);
                    string mot = MainWindow.Quran[id].Ayahs[verset.NumberInSurah - 1].Text.Split(' ')[i];
                    if(i < verset.Text.Split(' ').Length - 1) // on ne met pas d'espacement avec un numéro de verset
                        textBlock_mot.Text = mot.Trim() + new String(' ', Convert.ToInt16(textBlock_espacement.Text));
                    else
                        textBlock_mot.Text = mot.Trim() + new String(' ', 2);

                    textBlock_mot.Cursor = Cursors.Hand;

                    SetUpBasicTextBlockWordEvent(textBlock_mot, Brushes.Tomato);

                    WrapPanel_QuranText.Children.Add(textBlock_mot);
                }

                TextBlock textBlock_finVerset = new TextBlock();
                textBlock_finVerset.Text = "﴿" + Utilities.ConvertNumeralsToArabic(verset.NumberInSurah.ToString()) + "﴾" + new String(' ', Convert.ToInt16(textBlock_espacement.Text));
                textBlock_finVerset.FontSize = Convert.ToInt16(textBlock_textSize.Text);
                textBlock_finVerset.Cursor = Cursors.Hand;
                textBlock_finVerset.ToolTip = verset.NumberInSurah;
                SetUpBasicTextBlockWordEvent(textBlock_finVerset, Brushes.Green);
                // les numéros toujours vont s'afficher avec la police me_quran
                if (!MainWindow.CurrentFont.ToString().Contains("ScheherazadeRegOT") && !MainWindow.CurrentFont.ToString().Contains("Othmani"))
                    textBlock_finVerset.FontFamily = (FontFamily)FindResource("Me_Quran");

                WrapPanel_QuranText.Children.Add(textBlock_finVerset);
            }

            this.Tag = id;
        }

        private void SetUpBasicTextBlockWordEvent(TextBlock textBlock, Brush mouseHoverColor)
        {         
            textBlock.MouseEnter += (sender, e) =>
            {
                textBlock.Foreground = mouseHoverColor;
            };

            textBlock.MouseLeave += (sender, e) =>
            {
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
    }
}
