using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static List<Sourate>? Quran = new List<Sourate>();
        private List<string> DownloadLinks = new List<string>(); // Contient tous les liens à télécharger
        private int CurrentDownloadIndex;
        private WebClient WcDownloader = new WebClient();

        public MainWindow()
        {
            InitializeComponent();

            InitQuran();

            // Ajoute un à la progressBar et passe au téléchargement suivant
            WcDownloader.DownloadFileCompleted += (sender, e) =>
            {
                if (CurrentDownloadIndex == DownloadLinks.Count - 1)
                {
                    progressBar_downloader.Value = 0;
                    Quran[Convert.ToInt16(userControl_QuranReader.Tag)].Downloaded = true;
                    File.WriteAllText(@"data\quran.json", JsonConvert.SerializeObject(Quran, Formatting.Indented));
                }
                else
                {
                    progressBar_downloader.Value += 1;
                    CurrentDownloadIndex++;
                    DownloadAll();
                }
            };
        }

        private void InitQuran()
        {
            Quran = JsonConvert.DeserializeObject<List<Sourate>>(File.ReadAllText(@"data\quran.json"));
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            userControl_QuranReader.AfficherSourate(0);
            if (Quran[0].Downloaded == true)
                checkBox_HorsLigne.IsChecked = true;
            else
                checkBox_HorsLigne.IsChecked = false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        /// <summary>
        /// Télécharge la sourate actuellement affiché 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_HorsLigne_Checked(object sender, RoutedEventArgs e)
        {
            // Tag du userControl = index de la sourate actuellement affiché 
            int surahId = Convert.ToInt16(userControl_QuranReader.Tag);

            if (Quran[surahId].Downloaded == false)
            {
                // Créé le dossier qui contient les audios
                Directory.CreateDirectory(@"data\quran\" + Quran[surahId].Transliteration);
                Directory.CreateDirectory(@"data\quran\" + Quran[surahId].Transliteration + @"\wbw"); // dossier contenant les audios de chaque mot
                Directory.CreateDirectory(@"data\quran\" + Quran[surahId].Transliteration + @"\verse"); // dossier contenant les audios de chaque verset

                DownloadLinks = new List<string>();

                // Ajoute les liens a télécharger dans la liste DownloadLinks
                foreach (var verse in Quran[surahId].Verses)
                {
                    // Audio du verset :
                    // https://verses.quran.com/Alafasy/ogg/001001.ogg

                    DownloadLinks.Add(
                    "https://verses.quran.com/Alafasy/ogg/" + (surahId + 1).ToString().PadLeft(3, '0') + verse.Id.ToString().PadLeft(3, '0') + ".ogg"
                    );

                    // Nombre de mot dans le verset
                    for (int i = 1; i <= verse.Text.Split(' ').Length; i++)
                    {
                        // Mot du verset :
                        // https://audio.qurancdn.com/wbw/001_001_001.mp3

                        DownloadLinks.Add(
                            "https://audio.qurancdn.com/wbw/" + (surahId + 1).ToString().PadLeft(3, '0') + "_" + verse.Id.ToString().PadLeft(3, '0') + "_" + i.ToString().PadLeft(3, '0') + ".mp3"
                            );
                    }
                }

                progressBar_downloader.Maximum = DownloadLinks.Count;
                CurrentDownloadIndex = 0;
                DownloadAll();
            }
        }

        /// <summary>
        /// Télécharge tous les liens présent dans DownloadLinks
        /// </summary>
        private void DownloadAll()
        {
            WcDownloader.DownloadFileAsync(new Uri(DownloadLinks[CurrentDownloadIndex]),

                // Set le path de téléchargement du fichier
                DownloadLinks[CurrentDownloadIndex].Contains("wbw")
                    ? @"data\quran\" + Quran[Convert.ToInt16(userControl_QuranReader.Tag)].Transliteration + @"\wbw\" + DownloadLinks[CurrentDownloadIndex].Substring(35, 7) + ".mp3"
                    : @"data\quran\" + Quran[Convert.ToInt16(userControl_QuranReader.Tag)].Transliteration + @"\verse\" + DownloadLinks[CurrentDownloadIndex].Substring(40, 3) + ".ogg"
                );
        }

        /// <summary>
        /// Supprime la sourate actuellement affiché des téléchargements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_HorsLigne_Unchecked(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Êtes-vous sûre de vouloir supprimer cette sourate des téléchargements ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Directory.Delete(@"data\quran\" + Quran[Convert.ToInt16(userControl_QuranReader.Tag)].Transliteration, true);

                Quran[Convert.ToInt16(userControl_QuranReader.Tag)].Downloaded = false;
                File.WriteAllText(@"data\quran.json", JsonConvert.SerializeObject(Quran, Formatting.Indented));
            }
            else
                checkBox_HorsLigne.IsChecked = true;
        }
    }
}
