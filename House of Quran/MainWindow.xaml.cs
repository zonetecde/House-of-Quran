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
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace House_of_Quran
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static List<Surah>? Quran = new List<Surah>();
        private WebClient WcDownloader = new WebClient();
        internal List<Recitateur> Recitateurs;
        internal static FontFamily CurrentFont;
        internal static MainWindow _MainWindow;
        private Timer T_InternetCheck = new Timer(1000);
        internal static bool HaveInternet = true;

        public MainWindow()
        {
            if (Properties.Settings.Default.FromWhereToWhereForEachSurah == null)
                Properties.Settings.Default.FromWhereToWhereForEachSurah = new System.Collections.Specialized.StringCollection();

            if (Properties.Settings.Default.DownloadList == null)
                Properties.Settings.Default.DownloadList = new System.Collections.Specialized.StringCollection();

            Properties.Settings.Default.Save();

            InitializeComponent();

            _MainWindow = this;

            InitFont();

            InitQuran();

            InitRécitateur();

            InitSurahComboBox();

            InitFontComboBox();

            comboBox_Recitateur.SelectionChanged += comboBox_Recitateur_SelectionChanged;
            comboBox_font.SelectionChanged += comboBox_font_SelectionChanged;

            WcDownloader.DownloadFileCompleted += DownloadFileCompleted;

            T_InternetCheck.Elapsed += new ElapsedEventHandler(InternetCheck);
            T_InternetCheck.Start();
        }

        private void InternetCheck(object? sender, ElapsedEventArgs e)
        {
            if (Utilities.CheckForInternetConnection())
            {
                if(!HaveInternet)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        HaveInternet = true;

                        // on change tout ce qu'il y a d'accessible qu'avec la wifi
                        ColorEffectOnDownloadedRecitator((int)userControl_QuranReader.Tag);
                        ColorEffectOnDownloadedSourate();
                        ContinueDownloading();
                        checkBox_HorsLigne.IsEnabled = true;
                    });

                }
                else
                    HaveInternet = true;
            }
            else
            {
                if(HaveInternet)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        HaveInternet = false;

                        // on change tout ce qu'il y a d'accessible qu'avec la wifi
                        ColorEffectOnDownloadedRecitator((int)userControl_QuranReader.Tag);
                        ColorEffectOnDownloadedSourate();
                        checkBox_HorsLigne.IsEnabled = false;
                    });
                }
                else
                    HaveInternet = false;
            }
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
                comboBox_Sourate.Items.Add(new ComboBoxItem() { Content = sourate.Number + ". " + sourate.EnglishName });
            // Par défaut : sourate de la dernière fermeture du logiciel
            comboBox_Sourate.SelectedIndex = Properties.Settings.Default.DerniereSourate;

            ColorEffectOnDownloadedSourate();
        }

        private void InitRécitateur()
        {
            // Ajoute les récitateurs à la comboBox
            Recitateurs = JsonConvert.DeserializeObject<List<Recitateur>>(File.ReadAllText(@"data\recitateur.json"));
            foreach (var recitateur in Recitateurs)
            {
                comboBox_Recitateur.Items.Add(new ComboBoxItem() { Content = recitateur.Nom });
            }
            comboBox_Recitateur.SelectedIndex = Properties.Settings.Default.DernierRecitateur; // Alafasy par défaut
        }

        /// <summary>
        /// Un audio de la liste d'audio à télécharger a été téléchargé.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
        {
            if (Properties.Settings.Default.CurrentDownloadIndex == Properties.Settings.Default.DownloadList.Count - 1)
            {
                progressBar_downloader.Value = 0;
                SaveQuran();
                Properties.Settings.Default.CurrentDownloadIndex = 0;
                Properties.Settings.Default.DownloadList.Clear();

                Properties.Settings.Default.Save();
            }
            else
            {
                // Ajoute un à la progressBar et passe au téléchargement suivant
                progressBar_downloader.Value += 1;
                Properties.Settings.Default.CurrentDownloadIndex++;
                string lien = Properties.Settings.Default.DownloadList[Properties.Settings.Default.CurrentDownloadIndex];

                if(!WcDownloader.IsBusy)
                    DownloadAll(Recitateurs.FindIndex(x => x.Lien == lien.Substring(0, lien.LastIndexOf('/') + 1)), lien.Contains("wbw") ? Convert.ToInt16(lien.Substring(31, 3)) : Convert.ToInt16(lien.Substring(lien.LastIndexOf('/') + 1, 3)));
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

            // Est-ce que le dernier téléchargement était fini ?
            ContinueDownloading();
        }

        private void ContinueDownloading()
        {
            if(HaveInternet)
                if (Properties.Settings.Default.DownloadList.Count > 0)
                {
                    string lien = Properties.Settings.Default.DownloadList[Properties.Settings.Default.CurrentDownloadIndex];
                    string lien2 = lien.Substring(0, lien.LastIndexOf('/') + 1);
                    progressBar_downloader.Maximum = Properties.Settings.Default.DownloadList.Count;
                    progressBar_downloader.Value = Properties.Settings.Default.CurrentDownloadIndex;
                    
                    DownloadAll(Recitateurs.FindIndex(x => x.Lien == lien.Substring(0, lien.LastIndexOf('/') + 1)), lien.Contains("wbw") ? Convert.ToInt16(lien.Substring(31, 3)) : Convert.ToInt16(lien.Substring(lien.LastIndexOf('/') + 1, 3)));
                }
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
            int surahId = (int)userControl_QuranReader.Tag;

            if (!Quran[surahId].DownloadedRecitateur.Contains(comboBox_Recitateur.SelectedIndex))
            {
                // Créé le dossier qui contient les audios
                Directory.CreateDirectory(@"data\quran\" + Quran[surahId].EnglishName);
                Directory.CreateDirectory(@"data\quran\" + Quran[surahId].EnglishName + @"\wbw"); // dossier contenant les audios de chaque mot
                Directory.CreateDirectory(@"data\quran\" + Quran[surahId].EnglishName + @"\verse"); // dossier contenant les audios de chaque verset

                // Ajoute les liens a télécharger dans la liste DownloadLinks
                foreach (var verse in Quran[surahId].Ayahs)
                {
                    string lienVerset = GetVerseDownloadLink(surahId, verse.NumberInSurah, comboBox_Recitateur.SelectedIndex);

                    if (!File.Exists(@"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName + @"\verse\" + comboBox_Recitateur.SelectedIndex + lienVerset.Substring(40, 3) + ".ogg")
                        && !Properties.Settings.Default.DownloadList.Contains(lienVerset))
                        Properties.Settings.Default.DownloadList.Add(lienVerset);

                    // Nombre de mot dans le verset
                    for (int i = 1; i <= verse.Text.Split(' ').Length; i++)
                    {
                        string lienWbw = GetWordDownloadLink(surahId, verse.NumberInSurah, i);

                        // S'il n'a pas déjà été téléchargé et qu'il n'est pas déjà dans la queue
                        if (!File.Exists(@"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName + @"\wbw\" + lienWbw.Substring(35, 7) + ".mp3")
                            && !Properties.Settings.Default.DownloadList.Contains(lienWbw))
                            Properties.Settings.Default.DownloadList.Add(lienWbw);
                    }
                }

                progressBar_downloader.Maximum = Properties.Settings.Default.DownloadList.Count;
                Quran[(int)userControl_QuranReader.Tag].DownloadedRecitateur.Add(comboBox_Recitateur.SelectedIndex); // Préviens que c'est téléchargé 
                SaveQuran();

                if(!WcDownloader.IsBusy)
                    DownloadAll(comboBox_Recitateur.SelectedIndex, (int)userControl_QuranReader.Tag);
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
            return "https://audio.qurancdn.com/wbw/" + (surahId + 1).ToString().PadLeft(3, '0') + "_" + verseId.ToString().PadLeft(3, '0') + "_" + (motPos).ToString().PadLeft(3, '0') + ".mp3";
        }

        /// <summary>
        /// Télécharge tous les liens présent dans DownloadLinks
        /// </summary>
        private void DownloadAll(int recitateur, int sourateIndex)
        {
            if (HaveInternet)
            {
                string folderPath = string.Empty;

                if (Properties.Settings.Default.DownloadList[Properties.Settings.Default.CurrentDownloadIndex].Contains("wbw"))
                    folderPath = @"data\quran\" + Quran[sourateIndex - 1].EnglishName + @"\wbw\" + Properties.Settings.Default.DownloadList[Properties.Settings.Default.CurrentDownloadIndex].Substring(35, 7) + ".mp3";
                else
                    folderPath = @"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName + @"\verse\" + recitateur + "-" + Properties.Settings.Default.DownloadList[Properties.Settings.Default.CurrentDownloadIndex].Substring(Properties.Settings.Default.DownloadList[Properties.Settings.Default.CurrentDownloadIndex].LastIndexOf("/") + 4, 3) + Recitateurs[recitateur].Extension;

                if (!WcDownloader.IsBusy)
                    WcDownloader.DownloadFileAsync(new Uri(Properties.Settings.Default.DownloadList[Properties.Settings.Default.CurrentDownloadIndex]),

                        // Set le path de téléchargement du fichier
                        folderPath
                        , comboBox_Recitateur.SelectedIndex);
            }
        }

        /// <summary>
        /// Supprime la sourate actuellement affiché des téléchargements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_HorsLigne_Unchecked(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Êtes-vous sûre de vouloir supprimer la récitation de " + Recitateurs[comboBox_Recitateur.SelectedIndex].Nom + " de sourate " + Quran[(int)userControl_QuranReader.Tag].EnglishName + " des téléchargements ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Il n'y a qu'un récitateur de télécharger pour cette sourate, on peut donc tout supprimer
                try
                {
                    if (Quran[(int)userControl_QuranReader.Tag].DownloadedRecitateur.Count == 1)
                        Directory.Delete(@"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName, true);
                    else
                    {
                        // Un récitateur a supprimé uniquement car d'autres sont téléchargés
                        string rootFolderPath = @"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName + @"\verse";
                        string filesToDelete = comboBox_Recitateur.SelectedIndex + "-"; // Va supprimer les fichiers contenant le récitateur là uniquement
                        string[] fileList = Directory.GetFiles(rootFolderPath).ToList().FindAll(x => x.Contains(filesToDelete)).ToArray();
                        fileList = fileList.Reverse().ToArray();
                        foreach (string file in fileList)
                            File.Delete(file);
                    }
                }
                catch
                {
                    MessageBox.Show("Veuillez attendre la fin du téléchargement pour pouvoir la supprimer.", "Information", MessageBoxButton.OK, MessageBoxImage.Error);

                    checkBox_HorsLigne.IsChecked = true;
                    return;
                }

                Quran[(int)userControl_QuranReader.Tag].DownloadedRecitateur.Remove(comboBox_Recitateur.SelectedIndex);
                SaveQuran();

                // Affiche les récitateurs téléchargé de cette sourate en vert
                ColorEffectOnDownloadedRecitator(comboBox_Sourate.SelectedIndex);
                ColorEffectOnDownloadedSourate();
            }
            else
            {
                checkBox_HorsLigne.IsChecked = true;
            }
        }

        private static void SaveQuran()
        {
            File.WriteAllText(@"data\quran.json", JsonConvert.SerializeObject(Quran, Formatting.Indented));
        }

        private void comboBox_Sourate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkBox_HorsLigne.Checked -= checkBox_HorsLigne_Checked;
            checkBox_HorsLigne.Unchecked -= checkBox_HorsLigne_Unchecked;

            Surah sourateChoisi = Quran.FirstOrDefault(x => x.Number - 1 == comboBox_Sourate.SelectedIndex);
            if (sourateChoisi != default)
            {

                userControl_QuranReader.AfficherSourate(sourateChoisi.Number - 1);

                if (Quran[sourateChoisi.Number - 1].DownloadedRecitateur.Contains(comboBox_Recitateur.SelectedIndex))
                    checkBox_HorsLigne.IsChecked = true;
                else
                    checkBox_HorsLigne.IsChecked = false;

                Properties.Settings.Default.DerniereSourate = sourateChoisi.Number - 1;

                // Affiche les récitateurs téléchargé de cette sourate en vert
                ColorEffectOnDownloadedRecitator(sourateChoisi.Number - 1);

                // Options 
                UpDownControl_endVerse.ValueChanged -= UpDownControl_verses_ValueChanged;
                UpDownControl_startVerse.ValueChanged -= UpDownControl_verses_ValueChanged;

                UpDownControl_endVerse.Value = sourateChoisi.Ayahs.Count;
                UpDownControl_endVerse.Maximum = sourateChoisi.Ayahs.Count;
                textBlock_maxVerset.Text = "à : (max : "+ sourateChoisi.Ayahs.Count + ")";
                UpDownControl_startVerse.Maximum = sourateChoisi.Ayahs.Count;
                UpDownControl_startVerse.Value = 1;
                UpDownControl_endVerse.Value = sourateChoisi.Ayahs.Count;

                // Set les dernières bornes mise sur cette sourate 
                string t = Properties.Settings.Default.FromWhereToWhereForEachSurah.Cast<string>().ToList().FirstOrDefault(x => x.Split(',')[0] == userControl_QuranReader.Tag.ToString());
                if (t != default)
                {
                    userControl_QuranReader.BorneChanged(Convert.ToInt16(t.Split(',')[1].Split('-')[0]), Convert.ToInt16(t.Split(',')[1].Split('-')[1]));
                    UpDownControl_startVerse.Value = Convert.ToInt16(t.Split(',')[1].Split('-')[0]);
                    UpDownControl_endVerse.Value = Convert.ToInt16(t.Split(',')[1].Split('-')[1]);
                }

                UpDownControl_endVerse.ValueChanged += UpDownControl_verses_ValueChanged;
                UpDownControl_startVerse.ValueChanged += UpDownControl_verses_ValueChanged;
            }

            AudioUtilities.PauseAllPlayingAudio();

            checkBox_HorsLigne.Checked += checkBox_HorsLigne_Checked;
            checkBox_HorsLigne.Unchecked += checkBox_HorsLigne_Unchecked;
        }

        private void ColorEffectOnDownloadedRecitator(int sourateChoisi)
        {
            for (int i = 0; i < comboBox_Recitateur.Items.Count; i++)
            {
                ComboBoxItem item = (ComboBoxItem)comboBox_Recitateur.Items[i];
                if (Quran[sourateChoisi].DownloadedRecitateur.Any(x => x == i))
                {
                    // Effet couleur car téléchargé
                    item.Foreground = Brushes.DarkGreen;
                    item.Background = Brushes.Bisque;
                    item.IsEnabled = true;
                }
                else
                {
                    item.Background = Brushes.Transparent;
                    item.Foreground = Brushes.Black;

                    if (!HaveInternet)
                        item.IsEnabled = false;
                    else
                        item.IsEnabled = true;
                }

            }
        }
        
        private void ColorEffectOnDownloadedSourate()
        {
            for (int i = 0; i < comboBox_Sourate.Items.Count; i++)
            {
                ComboBoxItem item = (ComboBoxItem)comboBox_Sourate.Items[i];
                if (Quran[i].DownloadedRecitateur.Any())
                {
                    // Effet couleur car téléchargé
                    item.Foreground = Brushes.DarkGreen;
                    item.Background = Brushes.Bisque;

                    item.IsEnabled = true;
                }
                else
                {
                    item.Background = Brushes.Transparent;
                    item.Foreground = Brushes.Black;

                    if (!HaveInternet)
                        item.IsEnabled = false;
                    else
                        item.IsEnabled = true;
                }
            }
        }

        private void comboBox_Recitateur_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkBox_HorsLigne.Checked -= checkBox_HorsLigne_Checked;
            checkBox_HorsLigne.Unchecked -= checkBox_HorsLigne_Unchecked;

            if (Quran[(int)userControl_QuranReader.Tag].DownloadedRecitateur.Contains(comboBox_Recitateur.SelectedIndex))
            {
                // vérifie que le récitateur est bien téléchargé 
                if(Directory.GetFiles(@"data\quran\" + Quran[(int)userControl_QuranReader.Tag].EnglishName + @"\verse").Any(x => x.Contains(comboBox_Recitateur.SelectedIndex + "-")))
                    checkBox_HorsLigne.IsChecked = true;
                else
                {
                    checkBox_HorsLigne.IsChecked = false;
                    Quran[(int)userControl_QuranReader.Tag].DownloadedRecitateur.Remove(comboBox_Recitateur.SelectedIndex);
                    SaveQuran();
                }
            }
            else
                checkBox_HorsLigne.IsChecked = false;

            Properties.Settings.Default.DernierRecitateur = comboBox_Recitateur.SelectedIndex;
            Properties.Settings.Default.Save();

            AudioUtilities.PauseAllPlayingAudio();

            checkBox_HorsLigne.Checked += checkBox_HorsLigne_Checked;
            checkBox_HorsLigne.Unchecked += checkBox_HorsLigne_Unchecked;
        }

        private void comboBox_font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.FontFamily = (FontFamily)FindResource((e.AddedItems[0] as ComboBoxItem).Content as string);
            userControl_QuranReader.CheckFontOfNumber(this.FontFamily);

            CurrentFont = this.FontFamily;
            Properties.Settings.Default.DernierePolice = comboBox_font.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            AudioUtilities.PauseAllPlayingAudio();
            Properties.Settings.Default.Save();
            SaveQuran();
        }

        private void UpDownControl_verses_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (UpDownControl_endVerse.Value < UpDownControl_startVerse.Value)
                UpDownControl_endVerse.Value++;

            userControl_QuranReader.BorneChanged(UpDownControl_startVerse.Value, UpDownControl_endVerse.Value);
            string t = Properties.Settings.Default.FromWhereToWhereForEachSurah.Cast<string>().ToList().FirstOrDefault(x => x.Split(',')[0] == userControl_QuranReader.Tag.ToString());
            if(t == default)
            {
                Properties.Settings.Default.FromWhereToWhereForEachSurah.Add(userControl_QuranReader.Tag.ToString() + "," + UpDownControl_startVerse.Value + "-" + UpDownControl_endVerse.Value);
            }
            else
            {
                Properties.Settings.Default.FromWhereToWhereForEachSurah[
                    Properties.Settings.Default.FromWhereToWhereForEachSurah.IndexOf(t)
                ] = userControl_QuranReader.Tag.ToString() + "," + UpDownControl_startVerse.Value + "-" + UpDownControl_endVerse.Value;
            }

            Properties.Settings.Default.Save();
        }
    }
}
