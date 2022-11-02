using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        internal static List<Surah>? Quran = new List<Surah>();
        private List<string> DownloadLinks = new List<string>(); // Contient tous les liens à télécharger
        private int CurrentDownloadIndex;
        private WebClient WcDownloader = new WebClient();
        private List<Recitateur> Recitateurs;
        private bool CheckedByUser = true; // Prevent la checkBox d'executer son event si ce n'est pas l'utilisateur qui lui a demandé de le faire
        internal static FontFamily CurrentFont;

        public MainWindow()
        {
            InitializeComponent();

            InitFont();

            InitQuran();

            InitRécitateur();

            InitSurahComboBox();

            InitFontComboBox();

            comboBox_Recitateur.SelectionChanged += comboBox_Recitateur_SelectionChanged;
            comboBox_font.SelectionChanged += comboBox_font_SelectionChanged;

            WcDownloader.DownloadFileCompleted += DownloadFileCompleted;

        }

        private void InitFont()
        {
            this.FontFamily = (FontFamily)FindResource((comboBox_font.Items[Properties.Settings.Default.DernierePolice] as ComboBoxItem).Content as string);
            CurrentFont = this.FontFamily;
            comboBox_font.SelectedIndex = Properties.Settings.Default.DernierePolice;
        }

        private void InitFontComboBox()
        {
            
        }

        private void InitSurahComboBox()
        {
            // Ajoute les sourates à la comboBox pour pouvoir en choisir une
            foreach (var sourate in Quran)
                comboBox_Sourate.Items.Add(sourate.Number + ". " + sourate.EnglishName);
            // Par défaut : sourate de la dernière fermeture du logiciel
            comboBox_Sourate.SelectedIndex = Properties.Settings.Default.DerniereSourate;
        }

        private void InitRécitateur()
        {
            // Ajoute les récitateurs à la comboBox
            Recitateurs = JsonConvert.DeserializeObject<List<Recitateur>>(File.ReadAllText(@"data\recitateur.json"));
            foreach (var recitateur in Recitateurs)
                comboBox_Recitateur.Items.Add(recitateur.Nom + (String.IsNullOrEmpty(recitateur.Type) ? String.Empty : " (" + recitateur.Type + ")"));
            comboBox_Recitateur.SelectedIndex = Properties.Settings.Default.DernierRecitateur; // Alafasy par défaut
        }

        /// <summary>
        /// Un audio de la liste d'audio à télécharger a été téléchargé.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
        {
            if (CurrentDownloadIndex == DownloadLinks.Count - 1)
            {
                progressBar_downloader.Value = 0;
                Quran[(int)userControl_QuranReader.Tag].DownloadedRecitateur.Add((int)e.UserState);
                File.WriteAllText(@"data\quran.json", JsonConvert.SerializeObject(Quran, Formatting.Indented));
            }
            else
            {
                // Ajoute un à la progressBar et passe au téléchargement suivant
                progressBar_downloader.Value += 1;
                CurrentDownloadIndex++;
                DownloadAll((int)e.UserState);
            }      
        }

        private void InitQuran()
        {
            Quran = JsonConvert.DeserializeObject<List<Surah>>(File.ReadAllText(@"data\quran.json"));
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //Utilities.GetQuranFromInternet();
            //Utilities.RecitateurToJson();
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
            if (!CheckedByUser)
            {
                CheckedByUser = true;
                return;
            }

            // Tag du userControl = index de la sourate actuellement affiché 
            int surahId = (int)userControl_QuranReader.Tag;

            if (!Quran[surahId].DownloadedRecitateur.Contains(comboBox_Recitateur.SelectedIndex))
            {
                // Créé le dossier qui contient les audios
                Directory.CreateDirectory(@"data\quran\" + Quran[surahId].EnglishName);
                Directory.CreateDirectory(@"data\quran\" + Quran[surahId].EnglishName + @"\wbw"); // dossier contenant les audios de chaque mot
                Directory.CreateDirectory(@"data\quran\" + Quran[surahId].EnglishName + @"\verse"); // dossier contenant les audios de chaque verset

                DownloadLinks = new List<string>();

                // Ajoute les liens a télécharger dans la liste DownloadLinks
                foreach (var verse in Quran[surahId].Ayahs)
                {
                    string lienVerset = GetVerseDownloadLink(surahId, verse.NumberInSurah, comboBox_Recitateur.SelectedIndex);

                    if (!File.Exists(@"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName + @"\verse\" + comboBox_Recitateur.SelectedIndex + lienVerset.Substring(40, 3) + ".ogg"))
                        DownloadLinks.Add(lienVerset);

                    // Nombre de mot dans le verset
                    for (int i = 1; i <= verse.Text.Split(' ').Length; i++)
                    {
                        string lienWbw = GetWordDownloadLink(surahId, verse.NumberInSurah, i);

                        // S'il n'a pas déjà été téléchargé 
                        if (!File.Exists(@"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName + @"\wbw\" + lienWbw.Substring(35, 7) + ".mp3"))
                            DownloadLinks.Add(lienWbw);
                    }
                }

                progressBar_downloader.Maximum = DownloadLinks.Count;
                CurrentDownloadIndex = 0;
                DownloadAll(comboBox_Recitateur.SelectedIndex);
            }
        }

        /// <summary>
        /// Renvois le lien de téléchargement de l'audio du verset
        /// </summary>
        /// <param name="surahId">Sourate pos</param>
        /// <param name="versePos">Verse pos dans sourate</param>
        /// <returns></returns>
        private string GetVerseDownloadLink(int surahId, int versePos, int recitateur)
        {
            // Audio du verset :
            return Recitateurs[recitateur].Lien + (surahId + 1).ToString().PadLeft(3, '0') + versePos.ToString().PadLeft(3, '0') + Recitateurs[recitateur].Extension;
        }

        /// <summary>
        /// Renvois le lien de téléchargement de l'audio du mot
        /// </summary>
        /// <param name="surahId">Sourate pos</param>
        /// <param name="verseId">Verse pos dans sourate</param>
        /// <param name="motPos">Mot pos dans verset</param>
        /// <returns></returns>
        private string GetWordDownloadLink(int surahId, int verseId, int motPos)
        {
            // Mot du verset :
            return "https://audio.qurancdn.com/wbw/" + (surahId + 1).ToString().PadLeft(3, '0') + "_" + verseId.ToString().PadLeft(3, '0') + "_" + motPos.ToString().PadLeft(3, '0') + ".mp3";
        }

        /// <summary>
        /// Télécharge tous les liens présent dans DownloadLinks
        /// </summary>
        private void DownloadAll(int recitateur)
        {
            string wPath = @"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName + @"\wbw\" + DownloadLinks[CurrentDownloadIndex].Substring(35, 7) + ".mp3";
            string vPath = @"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName + @"\verse\" + recitateur + "-" + DownloadLinks[CurrentDownloadIndex].Substring(DownloadLinks[CurrentDownloadIndex].LastIndexOf("/") + 4, 3) + Recitateurs[recitateur].Extension;

            WcDownloader.DownloadFileAsync(new Uri(DownloadLinks[CurrentDownloadIndex]),

                // Set le path de téléchargement du fichier
                DownloadLinks[CurrentDownloadIndex].Contains("wbw")
                    ? wPath
                    : vPath
                , comboBox_Recitateur.SelectedIndex);
        }

        /// <summary>
        /// Supprime la sourate actuellement affiché des téléchargements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_HorsLigne_Unchecked(object sender, RoutedEventArgs e)
        {
            if(!CheckedByUser)
            {
                CheckedByUser = true;
                return;
            }

            if (MessageBox.Show("Êtes-vous sûre de vouloir supprimer la récitation de " + Recitateurs[comboBox_Recitateur.SelectedIndex].Nom + " " + Recitateurs[comboBox_Recitateur.SelectedIndex].Type + " des téléchargements ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Il n'y a qu'un récitateur de télécharger pour cette sourate, on peut donc tout supprimer
                if (Quran[(int)userControl_QuranReader.Tag].DownloadedRecitateur.Count == 1)
                    Directory.Delete(@"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName, true);
                else
                {
                    // Un récitateur a supprimé uniquement car d'autres sont téléchargés

                    string rootFolderPath = @"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName + @"\verse";
                    string filesToDelete = comboBox_Recitateur.SelectedIndex + "-"; // Va supprimer les fichiers contenant le récitateur là uniquement
                    string[] fileList = Directory.GetFiles(rootFolderPath).ToList().FindAll(x => x.Contains(filesToDelete)).ToArray();
                    foreach (string file in fileList)                 
                        File.Delete(file);                
                }

                Quran[(int)userControl_QuranReader.Tag].DownloadedRecitateur.Remove(comboBox_Recitateur.SelectedIndex);
                File.WriteAllText(@"data\quran.json", JsonConvert.SerializeObject(Quran, Formatting.Indented));
            }
            else
            {
                CheckedByUser = false;
                checkBox_HorsLigne.IsChecked = true;
            }
        }

        private void comboBox_Sourate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Surah sourateChoisi = Quran.FirstOrDefault(x => x.Number - 1 == comboBox_Sourate.SelectedIndex);
            if (sourateChoisi != default)
            {
                CheckedByUser = false;

                userControl_QuranReader.AfficherSourate(sourateChoisi.Number - 1);
                if (Quran[sourateChoisi.Number - 1].DownloadedRecitateur.Contains(comboBox_Recitateur.SelectedIndex))
                    checkBox_HorsLigne.IsChecked = true;
                else
                    checkBox_HorsLigne.IsChecked = false;

                Properties.Settings.Default.DerniereSourate = sourateChoisi.Number - 1;
                Properties.Settings.Default.Save();
            }
        }

        private void comboBox_Recitateur_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckedByUser = false;

            if (Quran[(int)userControl_QuranReader.Tag].DownloadedRecitateur.Contains(comboBox_Recitateur.SelectedIndex))
                checkBox_HorsLigne.IsChecked = true;
            else
                checkBox_HorsLigne.IsChecked = false;

            Properties.Settings.Default.DernierRecitateur = comboBox_Recitateur.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void comboBox_font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.FontFamily = (FontFamily)FindResource((e.AddedItems[0] as ComboBoxItem).Content as string);
            userControl_QuranReader.CheckFontOfNumber(this.FontFamily);

            CurrentFont = this.FontFamily;
            Properties.Settings.Default.DernierePolice = comboBox_font.SelectedIndex;
            Properties.Settings.Default.Save();
        }
    }
}
