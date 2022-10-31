using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public MainWindow()
        {
            InitializeComponent();

            InitQuran();
        }

        private void InitQuran()
        {
            Quran = JsonConvert.DeserializeObject<List<Sourate>>(File.ReadAllText(@"data\quran.json"));
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            userControl_QuranReader.ShowSourate(0);
        }
    }
}
