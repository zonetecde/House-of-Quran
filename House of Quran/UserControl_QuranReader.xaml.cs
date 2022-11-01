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
        }

        public void AfficherSourate(int id)
        {
            // Set sourate Header
            txtBlock_SourateNomArabe.Text = "  سورة" + MainWindow.Quran[id].Name;
            txtBlock_SourateNombreVerset.Text = MainWindow.Quran[id].Verses.Count + " versets";
            txtBlock_typeSourate.Text = MainWindow.Quran[id].Type == "meccan" ? "Mecquoise" : "Médinoise";

            // Affiche les versets
            foreach (Verse verset in MainWindow.Quran[id].Verses)
            {
                for (int i = 0; i < verset.Text.Split(' ').Length; i++)
                {
                    TextBlock textBlock_mot = new TextBlock();
                    textBlock_mot.FontSize = 38;
                    textBlock_mot.Text = MainWindow.Quran[id].Verses[verset.Id - 1].Text.Split(' ')[i] + "    ";
                    textBlock_mot.Cursor = Cursors.Hand;

                    SetUpBasicTextBlockWordEvent(textBlock_mot, Brushes.Tomato);

                    WrapPanel_QuranText.Children.Add(textBlock_mot);
                }

                TextBlock textBlock_finVerset = new TextBlock();
                textBlock_finVerset.Text = "  ﴿" + Utilities.ConvertNumeralsToArabic(verset.Id.ToString()) + "﴾     ";
                textBlock_finVerset.FontSize = 40;
                textBlock_finVerset.Cursor = Cursors.Hand;

                SetUpBasicTextBlockWordEvent(textBlock_finVerset, Brushes.Green);

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

        private void Button_SizeChanger_Click(object sender, RoutedEventArgs e)
        {
            int currentSize = Convert.ToInt16(textBlock_textSize.Text);
            if ((sender as Button).Content.ToString() == "+")
                currentSize++;
            else
                currentSize--;

            if (currentSize < 1)
                currentSize = 1;
            else if (currentSize > 255)
                currentSize = 255;

            textBlock_textSize.Text = currentSize.ToString();

            foreach (TextBlock textBlock_mot in WrapPanel_QuranText.Children)
            {
                textBlock_mot.FontSize = Convert.ToInt16(textBlock_textSize.Text);
            }
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
    }
}
