using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace House_of_Quran
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Ayah
    {
        public int Number { get; set; }
        public string Text { get; set; }
        public int NumberInSurah { get; set; }
        public int Juz { get; set; }
        public int Manzil { get; set; }
        public int Page { get; set; }
        public int Ruku { get; set; }
        public int HizbQuarter { get; set; }
        public object Sajda { get; set; }
    }

    //public class Data
    //{
    //    public List<Surah> Surahs { get; set; }
    //    public Edition Edition { get; set; }
    //}

    //public class Edition
    //{
    //    public string Identifier { get; set; }
    //    public string Language { get; set; }
    //    public string Name { get; set; }
    //    public string EnglishName { get; set; }
    //    public string Format { get; set; }
    //    public string Type { get; set; }
    //}

    //public class Root
    //{
    //    public int code { get; set; }
    //    public string status { get; set; }
    //    public Data data { get; set; }
    //}

    public class Surah
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string EnglishName { get; set; }
        public string EnglishNameTranslation { get; set; }
        public string RevelationType { get; set; }
        public List<Ayah> Ayahs { get; set; }
        public List<int> DownloadedRecitateur { get; set; }
    }
}
