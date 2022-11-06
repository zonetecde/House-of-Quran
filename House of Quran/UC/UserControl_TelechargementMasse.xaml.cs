using System;
using System.Collections.Generic;
using System.Linq;
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
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace House_of_Quran
{
    /// <summary>
    /// Logique d'interaction pour UserControl_TelechargementMasse.xaml
    /// </summary>
    public partial class UserControl_TelechargementMasse : UserControl
    {
        public UserControl_TelechargementMasse()
        {
            InitializeComponent();


        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        internal void Load()
        {
            if (checkListBox_surah.Items.Count == 0 || checkListBox_recitateur.Items.Count == 0)
            {
                foreach (Surah surah in MainWindow.Quran!)
                {
                    checkListBox_surah.Items.Add(surah.Number + ". " + surah.EnglishName);
                }

                foreach (Recitateur recitateur in MainWindow._MainWindow!.Recitateurs)
                {
                    checkListBox_recitateur.Items.Add(recitateur.Nom);
                }
            }
        }

        private void txtBox_searchSura_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkListBox_surah.Items.Clear();

            if (String.IsNullOrEmpty(textBox_searchSurah.Text))
            {
                foreach (Surah surah in MainWindow.Quran!)
                    checkListBox_surah.Items.Add(surah.Number + ". " + surah.EnglishName);
            }
            else
            {
                foreach (Surah surah in MainWindow.Quran!)
                    if(RemoveRepetition((surah.Number + ". " + surah.EnglishName).Replace(".", " ").Replace("-", " ").ToLower()).Contains(RemoveRepetition(textBox_searchSurah.Text.Replace("."," ").Replace("-", " ").ToLower())))
                        checkListBox_surah.Items.Add(surah.Number + ". " + surah.EnglishName);
            }

        }

        private string RemoveRepetition(string v)
        {
            string news = v[0].ToString();
            for (int i = 0; i < v.Length; i++)
            {
                if(i >= 1)
                    if (news[news.Length - 1] != v[i])
                        news += v[i];
            }
            return news;
        }

        private void Button_ToutCocher_Click(object sender, RoutedEventArgs e)
        {

            checkListBox_surah.SelectAll();

        }

        private void Button_ToutDecocher_Click(object sender, RoutedEventArgs e)
        {
            checkListBox_surah.UnSelectAll();
        }

        private void Button_ToutCocher2_Click(object sender, RoutedEventArgs e)
        {
            checkListBox_recitateur.SelectAll();
        }

        private void Button_ToutDecocher2_Click(object sender, RoutedEventArgs e)
        {
            checkListBox_recitateur.UnSelectAll();
        }

        /// <summary>
        /// Télécharge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (checkListBox_recitateur.SelectedItems.Count > 0 && checkListBox_surah.SelectedItems.Count > 0)
            {

                int sTag = Convert.ToInt16(MainWindow._MainWindow!.userControl_QuranReader.Tag.ToString());
                int sRec = MainWindow._MainWindow!.comboBox_Recitateur.SelectedIndex;
                bool first = true;

                foreach (var item in checkListBox_surah.SelectedItems)
                {
                    Surah s = MainWindow.Quran![Convert.ToInt16(item.ToString()!.Split('.')[0]) - 1];
                    MainWindow._MainWindow!.userControl_QuranReader.Tag = s.Number - 1;

                    foreach (var item_recitateur in checkListBox_recitateur.SelectedItems)
                    {
                        // Si la sourate actuellement affiché est en train de se faire télécharger
                        if(sTag == s.Number - 1 && MainWindow._MainWindow!.comboBox_Recitateur.SelectedIndex == MainWindow._MainWindow!.Recitateurs.FindIndex(x => x.Nom == item_recitateur.ToString()))
                        {
                            MainWindow._MainWindow!.checkBox_HorsLigne.Checked -= MainWindow._MainWindow!.checkBox_HorsLigne_Checked;
                            MainWindow._MainWindow!.checkBox_HorsLigne.IsChecked = true;
                            MainWindow._MainWindow!.checkBox_HorsLigne.Checked += MainWindow._MainWindow!.checkBox_HorsLigne_Checked;
                        }

                        MainWindow._MainWindow!.comboBox_Recitateur.SelectedIndex = MainWindow._MainWindow!.Recitateurs.FindIndex(x => x.Nom == item_recitateur.ToString());
                        if (first)
                        {
                            MainWindow._MainWindow!.checkBox_HorsLigne_Checked(this, new RoutedEventArgs());
                            first = false;
                        }
                        else
                            MainWindow._MainWindow!.checkBox_HorsLigne_Checked(this, null!);
                    }
                }
                MainWindow._MainWindow!.userControl_QuranReader.Tag = sTag;
                MainWindow._MainWindow!.comboBox_Recitateur.SelectedIndex = sRec;


                MainWindow._MainWindow!.Grid_telechargementMasse.Visibility = Visibility.Hidden;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner au moins une sourate à télécharger avec un récitateur.");
            }
        }

        private void textBox_searchRecitateur_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkListBox_recitateur.Items.Clear();

            if (String.IsNullOrEmpty(textBox_searchRecitateur.Text))
            {
                foreach (Recitateur recitateur in MainWindow._MainWindow!.Recitateurs)
                {
                    checkListBox_recitateur.Items.Add(recitateur.Nom);
                }
            }
            else
            {
                foreach (Recitateur recitateur in MainWindow._MainWindow!.Recitateurs)
                {
                    if (RemoveRepetition((recitateur.Nom).Replace(".", " ").Replace("-", " ").ToLower()).Contains(RemoveRepetition(textBox_searchRecitateur.Text.Replace(".", " ").Replace("-", " ").ToLower())))
                        checkListBox_recitateur.Items.Add(recitateur.Nom);
                }
            }
        }
    }
}
