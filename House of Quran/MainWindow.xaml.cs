using NAudio.Gui;
using NAudio.SoundFont;
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
        internal List<Recitateur> Recitateurs = new List<Recitateur>();
        internal static FontFamily? CurrentFont;
        internal static MainWindow? _MainWindow;
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

            checkbox_repeter_lecture.IsChecked = Properties.Settings.Default.RepeterLecture;
            checkbox_LectureAutomatique.IsChecked = Properties.Settings.Default.LectureAutomatique;
            integerUpDown_tempsRepeter.Value = (int)Properties.Settings.Default.TempsRepeter;
            integerUpDown_tempsMemoRepeter.Value = (int)Properties.Settings.Default.TempsRepeterMemo;

            switch (Properties.Settings.Default.ChoixRecitation)
            {
                case 1:
                    radioButton_recitation_wbwandverse.IsChecked = true;
                    break;
                case 2:
                    radioButton_recitation_wbw.IsChecked = true;
                    break;
                case 3:
                    radioButton_recitation_verse.IsChecked = true;
                    break;
            }

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
            InternetCheck(this, null!);

            //debug
            //Properties.Settings.Default.DownloadList.Clear();
            //Properties.Settings.Default.CurrentDownloadIndex = 0;
            //Properties.Settings.Default.Save();

            checkBox_tajweed.IsChecked = Properties.Settings.Default.Tajweed;
            checkbox_uniquementVerset.IsChecked = Properties.Settings.Default.UniquementPourVerset;
            radioButton_modeLecture.IsChecked = Properties.Settings.Default.ModeLecture;
            radioButton_modeMemorisation.IsChecked = !Properties.Settings.Default.ModeLecture;
            Border_modeLecture.IsEnabled = Properties.Settings.Default.ModeLecture;
            integerUpDown_tempsMemoRepeterSeconde.Value = Properties.Settings.Default.TempsRepeterMemoSeconde;
            integerUpDown_tempsRepeterSeconde.Value = Properties.Settings.Default.TempsRepeterSeconde;
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
                        //checkBox_tajweed.IsEnabled = true;
                        //checkBox_HorsLigne.IsEnabled = true;
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
                        //checkBox_HorsLigne.IsEnabled = false;
                        //checkBox_tajweed.IsEnabled = false;
                    });
                }
                else
                    HaveInternet = false;
            }
        }


        private void InitFont()
        {
            this.FontFamily = (FontFamily)FindResource(((ComboBoxItem)comboBox_font.Items[Properties.Settings.Default.DernierePolice]).Content as string);
            CurrentFont = this.FontFamily;
            comboBox_font.SelectedIndex = Properties.Settings.Default.DernierePolice;
        }

        private void InitFontComboBox()
        {
            
        }

        private void InitSurahComboBox()
        {
            // Ajoute les sourates à la comboBox pour pouvoir en choisir une
            foreach (var sourate in Quran!)
                comboBox_Sourate.Items.Add(new ComboBoxItem() { Content = sourate.Number + ". " + sourate.EnglishName });
            // Par défaut : sourate de la dernière fermeture du logiciel
            comboBox_Sourate.SelectedIndex = Properties.Settings.Default.DerniereSourate;

            ColorEffectOnDownloadedSourate();
        }

        private void InitRécitateur()
        {
            // Ajoute les récitateurs à la comboBox
            Recitateurs = JsonConvert.DeserializeObject<List<Recitateur>>(File.ReadAllText(@"data\recitateur.json"))!;
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
                string lien = Properties.Settings.Default.DownloadList[Properties.Settings.Default.CurrentDownloadIndex]!;

                if (!WcDownloader.IsBusy)
                    if (lien.Contains("data") || lien.Contains("wbw"))
                    {
                        DownloadAll(Recitateurs.FindIndex(x => x.Lien == lien.Substring(0, lien.LastIndexOf('/') + 1)), lien.Contains("wbw") ? Convert.ToInt16(lien.Substring(31, 3)) : Convert.ToInt16(lien.Substring(lien.LastIndexOf('/') + 1, 3)));
                    }
                    else
                    {
                        //.Substring(lien.Substring(46, 3).LastIndexOf('/'), lien.Substring(46, 3).Length - lien.Substring(46, 3).LastIndexOf('/'))
                        string a = lien.Substring(46, 3);
                        do
                        {
                            if (int.TryParse(a, out int b))
                            {
                                DownloadAll(0, b);
                                break;
                            }

                            a = a.Remove(a.Length - 1, 1);
                        }
                        while (true);
                    }

            }
        }

        private void InitQuran()
        {
            Quran = JsonConvert.DeserializeObject<List<Surah>>(File.ReadAllText(@"data\Quran.json"));
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //Utilities.GetQuranFromInternet();
            //Utilities.RecitateurToJson();
            //Utilities.GetQuran(); //from tanzil.net

            // Est-ce que le dernier téléchargement était fini ?
            ContinueDownloading();
        }

        private void ContinueDownloading()
        {
            if(HaveInternet)
                if (Properties.Settings.Default.DownloadList.Count > 0)
                {
                    string lien = Properties.Settings.Default.DownloadList[Properties.Settings.Default.CurrentDownloadIndex]!;
                    string lien2 = lien.Substring(0, lien.LastIndexOf('/') + 1);
                    progressBar_downloader.Maximum = Properties.Settings.Default.DownloadList.Count;
                    progressBar_downloader.Value = Properties.Settings.Default.CurrentDownloadIndex;

                    int recitateur = Recitateurs.FindIndex(x => x.Lien == lien.Substring(0, lien.LastIndexOf('/') + 1));
                    int sourate = lien.Contains("wbw") ? Convert.ToInt16(lien.Substring(31, 3)) : lien.Contains("data") ? Convert.ToInt16(lien.Substring(lien.LastIndexOf('/') + 1, 3)) : Convert.ToInt16((lien.Substring(lien.LastIndexOf("r/"), lien.Length
                      - lien.LastIndexOf("r/")).Replace("r/", string.Empty).Replace("/", "_")).Split('_')[0]);
                    DownloadAll(recitateur, sourate);
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
        internal void checkBox_HorsLigne_Checked(object sender, RoutedEventArgs e)
        {
            // Tag du userControl = index de la sourate actuellement affiché 
            int surahId = (int)userControl_QuranReader.Tag;

            if (!Quran![surahId].DownloadedRecitateur.Contains(comboBox_Recitateur.SelectedIndex))
            {
                // Créé le dossier qui contient les audios
                Directory.CreateDirectory(@"data\quran\" + Quran![surahId].EnglishName);
                Directory.CreateDirectory(@"data\quran\" + Quran![surahId].EnglishName + @"\wbw"); // dossier contenant les audios de chaque mot
                Directory.CreateDirectory(@"data\quran\" + Quran![surahId].EnglishName + @"\verse"); // dossier contenant les audios de chaque verset
                Directory.CreateDirectory(@"data\quran\" + Quran![surahId].EnglishName + @"\tajweed"); // dossier contenant les images tajweed

                // Ajoute les liens a télécharger dans la liste DownloadLinks
                foreach (var verse in Quran![surahId].Ayahs)
                {
                    string lienVerset = GetVerseDownloadLink(surahId, verse.NumberInSurah, comboBox_Recitateur.SelectedIndex);

                    if (!File.Exists(@"data\quran\" + Quran![(int)userControl_QuranReader.Tag].EnglishName + @"\verse\" + comboBox_Recitateur.SelectedIndex + lienVerset.Substring(40, 3) + ".ogg")
                        && !Properties.Settings.Default.DownloadList.Contains(lienVerset))
                        Properties.Settings.Default.DownloadList.Add(lienVerset);

                    // Nombre de mot dans le verset
                    for (int i = 1; i <= verse.Text.Split(' ').Length; i++)
                    {
                        string lienWbw = GetWordDownloadLink(surahId, verse.NumberInSurah, i);

                        // S'il n'a pas déjà été téléchargé et qu'il n'est pas déjà dans la queue
                        if (!File.Exists(@"data\quran\" + Quran![(int)userControl_QuranReader.Tag].EnglishName + @"\wbw\" + lienWbw.Substring(35, 7) + ".mp3")
                            && !Properties.Settings.Default.DownloadList.Contains(lienWbw))
                            Properties.Settings.Default.DownloadList.Add(lienWbw);
                    }

                    // tajweed
                    int toSub = 0;
                    int addToAudio = 0;
                    for (int i = 0; i < verse.Text.Split(' ').Length + addToAudio; i++)
                    {
                        try
                        {
                            string mot = MainWindow.Quran![surahId].Ayahs[verse.NumberInSurah - 1].Text.Split(' ')[i];


                            if (mot.Contains("ۘ") || mot.Contains("ۖ") || mot.Contains("ۗ") || mot.Contains("ۙ") || mot.Contains("ۚ") || mot.Contains("ۛ") || mot.Contains("ۜ"))
                            {
                                // C'est un signe de prononciation, l'audio du verset +1
                                addToAudio++;
                            }


                        if (MainWindow.Quran![surahId].Ayahs[verse.NumberInSurah - 1].Text.Split(' ').Length - 1 > i + 1)
                            if (mot == "يَا" && MainWindow.Quran![surahId].Ayahs[verse.NumberInSurah - 1].Text.Split(' ')[i + 1] == "أَيُّهَا")
                            {
                                toSub++;
                                i++;
                            }

                            string url_tajweed = "https://static.qurancdn.com/images/w/rq-color/" + (surahId + 1) + "/" + verse.NumberInSurah + "/" + ((i + 1) - toSub + addToAudio) + ".png";

                            if (!File.Exists(@"data\quran\" + Quran![(int)userControl_QuranReader.Tag].EnglishName + @"\tajweed\" + (surahId + 1) + "_" + verse.NumberInSurah + "_" + ((i + 1) - toSub)))
                            {
                                Properties.Settings.Default.DownloadList.Add(url_tajweed);
                            }
                        }
                        catch { /*mot dans + addToAudio, je met ça là pour l'ajout des mots après un petit djim  */

                            string lienWbw = GetWordDownloadLink(surahId, verse.NumberInSurah, i + 1);
                            Properties.Settings.Default.DownloadList.Add(lienWbw);
                        }


                    }

                }

                progressBar_downloader.Maximum = Properties.Settings.Default.DownloadList.Count;
                Quran![(int)userControl_QuranReader.Tag].DownloadedRecitateur.Add(comboBox_Recitateur.SelectedIndex); // Préviens que c'est téléchargé 
                SaveQuran();

                if(!WcDownloader.IsBusy)
                    DownloadAll(comboBox_Recitateur.SelectedIndex, (int)userControl_QuranReader.Tag);

                if(!HaveInternet && e != null)
                {
                    MessageBox.Show("Le téléchargement commencera lorsque vous serez connecté à internet", "Information");
                }
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
        private void DownloadAll(int recitateur = -1, int sourateIndex = -1)
        {
            if (HaveInternet)
            {
                string folderPath;

                string url = Properties.Settings.Default.DownloadList[Properties.Settings.Default.CurrentDownloadIndex]!;

                if (url.Contains("wbw")) // mot
                {
                    folderPath = @"data\quran\" + Quran![sourateIndex - 1].EnglishName + @"\wbw\" + url.Substring(35, 7) + ".mp3";
                }
                else if (url.Contains("data")) // verset
                {
                    folderPath = @"data\quran\" + Quran![(int)userControl_QuranReader.Tag].EnglishName + @"\verse\" + recitateur + "-" + url.Substring(url.LastIndexOf("/") + 4, 3) + Recitateurs[recitateur].Extension;

                }
                else
                {
                    folderPath = @"data\quran\" + Quran![(int)userControl_QuranReader.Tag].EnglishName + @"\tajweed\" +
                        url.Substring(url.LastIndexOf("r/"), url.Length
                      - url.LastIndexOf("r/")).Replace("r/", string.Empty).Replace("/", "_");
                }


                if (!WcDownloader.IsBusy)
                    WcDownloader.DownloadFileAsync(new Uri(url!),

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
            if (MessageBox.Show("Êtes-vous sûre de vouloir supprimer la récitation de " + Recitateurs[comboBox_Recitateur.SelectedIndex].Nom + " de sourate " + Quran![(int)userControl_QuranReader.Tag].EnglishName + " des téléchargements ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Il n'y a qu'un récitateur de télécharger pour cette sourate, on peut donc tout supprimer
                try
                {
                    if (Quran![(int)userControl_QuranReader.Tag].DownloadedRecitateur.Count == 1)
                        Directory.Delete(@"data\quran\" + Quran![(int)userControl_QuranReader.Tag].EnglishName, true);
                    else
                    {
                        // Un récitateur a supprimé uniquement car d'autres sont téléchargés
                        string rootFolderPath = @"data\quran\" + Quran![(int)userControl_QuranReader.Tag].EnglishName + @"\verse";
                        string filesToDelete = comboBox_Recitateur.SelectedIndex + "-"; // Va supprimer les fichiers contenant le récitateur là uniquement
                        string[] fileList = Directory.GetFiles(rootFolderPath).ToList().FindAll(x => x.Contains(filesToDelete)).ToArray();
                        fileList = fileList.Reverse().ToArray();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
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

                Quran![(int)userControl_QuranReader.Tag].DownloadedRecitateur.Remove(comboBox_Recitateur.SelectedIndex);
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
            File.WriteAllText(@"data\Quran.json", JsonConvert.SerializeObject(Quran, Formatting.Indented));
        }

        private void comboBox_Sourate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkBox_HorsLigne.Checked -= checkBox_HorsLigne_Checked;
            checkBox_HorsLigne.Unchecked -= checkBox_HorsLigne_Unchecked;

            Surah sourateChoisi = Quran!.FirstOrDefault(x => x.Number - 1 == comboBox_Sourate.SelectedIndex)!;
            if (sourateChoisi != default)
            {
                userControl_QuranReader.AfficherSourate(sourateChoisi.Number - 1);

                if (Quran![sourateChoisi.Number - 1].DownloadedRecitateur.Contains(comboBox_Recitateur.SelectedIndex))
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
                string t = Properties.Settings.Default.FromWhereToWhereForEachSurah.Cast<string>().ToList().FirstOrDefault(x => x.Split(',')[0] == userControl_QuranReader.Tag.ToString())!;
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
                if (Quran![sourateChoisi].DownloadedRecitateur.Any(x => x == i))
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
                if (Quran![i].DownloadedRecitateur.Any())
                {
                    // Effet couleur car téléchargé
                    item.Foreground = Brushes.DarkGreen;
                    item.Background = Brushes.Bisque;
                }
                else
                {
                    item.Background = Brushes.Transparent;
                    item.Foreground = Brushes.Black;
                }
            }
        }

        private void comboBox_Recitateur_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkBox_HorsLigne.Checked -= checkBox_HorsLigne_Checked;
            checkBox_HorsLigne.Unchecked -= checkBox_HorsLigne_Unchecked;

            if (Quran![(int)userControl_QuranReader.Tag].DownloadedRecitateur.Contains(comboBox_Recitateur.SelectedIndex))
            {
                // vérifie que le récitateur est bien téléchargé 
                if(Directory.GetFiles(@"data\quran\" + Quran![(int)userControl_QuranReader.Tag].EnglishName + @"\verse").Any(x => x.Contains(comboBox_Recitateur.SelectedIndex + "-")))
                    checkBox_HorsLigne.IsChecked = true;
                else
                {
                    checkBox_HorsLigne.IsChecked = false;
                    Quran![(int)userControl_QuranReader.Tag].DownloadedRecitateur.Remove(comboBox_Recitateur.SelectedIndex);
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
            this.FontFamily = (FontFamily)FindResource(((ComboBoxItem)e.AddedItems[0]!).Content as string);
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
            string t = Properties.Settings.Default.FromWhereToWhereForEachSurah.Cast<string>().ToList().FirstOrDefault(x => x.Split(',')[0] == userControl_QuranReader.Tag.ToString())!;
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

        /// <summary>
        /// Applique le tajweed ou le désactive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_tajweed_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Tajweed = checkBox_tajweed.IsChecked!.Value;
            Properties.Settings.Default.Save();

            userControl_QuranReader.ApplyTajweed(checkBox_tajweed.IsChecked.Value);
        }

        /// <summary>
        /// Si on utilise la fonction répéter on play obligatoirement en automatique
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkbox_repeter_lecture_Checked(object sender, RoutedEventArgs e)
        {
            checkbox_LectureAutomatique.IsChecked = true;
            checkbox_LectureAutomatique.IsEnabled = false;
            checkbox_uniquementVerset.IsEnabled = true;

            integerUpDown_tempsRepeter.IsEnabled = true;

            Properties.Settings.Default.LectureAutomatique = true;
            Properties.Settings.Default.RepeterLecture = true;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Enlève le forçage de la lecture automatique si on est avec le répéter 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkbox_repeter_lecture_Unchecked(object sender, RoutedEventArgs e)
        {
            checkbox_LectureAutomatique.IsEnabled = true;
            checkbox_uniquementVerset.IsEnabled = false;
            integerUpDown_tempsRepeter.IsEnabled = false;
            Properties.Settings.Default.LectureAutomatique = true;
            Properties.Settings.Default.RepeterLecture = false;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Save paramètre
        /// </summary>
        private void radioButtons_recitation_Checked(object sender, RoutedEventArgs e)
        {
            if((sender as RadioButton) == radioButton_recitation_wbwandverse)        
                Properties.Settings.Default.ChoixRecitation = 1;
            else if ((sender as RadioButton) == radioButton_recitation_wbw)
                Properties.Settings.Default.ChoixRecitation = 2;
            else
                Properties.Settings.Default.ChoixRecitation = 3;

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Save paramètre
        /// </summary>
        private void integerUpDown_tempsRepeter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                Properties.Settings.Default.TempsRepeter = (byte)integerUpDown_tempsRepeter.Value!.Value;
            }
            catch
            {
                Properties.Settings.Default.TempsRepeter = 0;
            }

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Save paramètre
        /// </summary>
        private void checkbox_LectureAutomatique_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LectureAutomatique =false;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Save paramètre
        /// </summary>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UniquementPourVerset = true;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Save paramètre
        /// </summary>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UniquementPourVerset = false;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Save paramètre
        /// </summary>
        private void RadioButton_ModeLecture_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ModeLecture = true;
            Border_modeLecture.IsEnabled = true;
            Border_modeMemorisation.IsEnabled = false;
            Properties.Settings.Default.Save();

            userControl_QuranReader.ActualPlayingTextBlockPos = -1;
            userControl_QuranReader.StopLecture();
        }

        /// <summary>
        /// Save paramètre
        /// </summary>
        private void RadioButton_ModeLecture_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ModeLecture = false;
            Border_modeLecture.IsEnabled = false;
            Border_modeMemorisation.IsEnabled = true;
            Properties.Settings.Default.Save();

            userControl_QuranReader.StopLecture();
        }

        private void integerUpDown_tempsMemoRepeter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                Properties.Settings.Default.TempsRepeterMemo = (byte)integerUpDown_tempsMemoRepeter.Value!.Value;
            }
            catch
            {
                Properties.Settings.Default.TempsRepeterMemo = 0;
            }

            Properties.Settings.Default.Save();
        }

        private void integerUpDown_tempsMemoRepeterSeconde_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                Properties.Settings.Default.TempsRepeterMemoSeconde = (byte)integerUpDown_tempsMemoRepeterSeconde.Value!.Value;
            }
            catch
            {
                Properties.Settings.Default.TempsRepeterMemoSeconde = 0;
            }

            Properties.Settings.Default.Save();
        }
        
        private void integerUpDown_tempsRepeterSeconde_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                Properties.Settings.Default.TempsRepeterSeconde = (byte)integerUpDown_tempsRepeterSeconde.Value!.Value;
            }
            catch
            {
                Properties.Settings.Default.TempsRepeterSeconde = 0;
            }

            Properties.Settings.Default.Save();
        }

        internal void AfficherTempsApprentissage(TimeSpan temps_total, bool enCalcul = false)
        {
            // apprentissage 
            if(!enCalcul)
                txtBlock_modeMemo.Text = "(" + (int)temps_total.Minutes + "min" + (int)temps_total.Seconds + ")";
            else
                txtBlock_modeMemo.Text = "(calcul...)";

            if (temps_total.TotalMilliseconds == 0)
                txtBlock_modeMemo.Text = "(aucune connexion internet)";
        }

        private void Button_TelechargementMasse_Click(object sender, RoutedEventArgs e)
        {
            uc_telechargementMasse.Load();
            Grid_telechargementMasse.Visibility = Visibility.Visible;
        }

        private void Grid_telechargementMasse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid_telechargementMasse.Visibility = Visibility.Hidden;
        }
    }
}
