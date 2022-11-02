﻿using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace House_of_Quran
{
    internal static class Utilities
    {
        internal static string ConvertNumeralsToArabic(string input)
        {
            return input = input.Replace('0', '٠')
                .Replace('1', '١')
                .Replace('2', '٢')
                .Replace('3', '٣')
                .Replace('4', '٤')
                .Replace('5', '٥')
                .Replace('6', '٦')
                .Replace('7', '٧')
                .Replace('8', '٨')
                .Replace('9', '٩');
        }

        internal static void RecitateurToJson()
        {
            List<Recitateur> recitateurs = new List<Recitateur>();
            //recitateurs.Add(new Recitateur("AbdulBaset", "Mujawwad", "https://verses.quran.com/AbdulBaset/Mujawwad/ogg/", ".ogg"));
            //recitateurs.Add(new Recitateur("AbdulBaset", "Murattal", "https://verses.quran.com/AbdulBaset/Murattal/ogg/", ".ogg"));
            //recitateurs.Add(new Recitateur("Husary", "Mujawwad", "https://verses.quran.com/Husary/Mujawwad/ogg/", ".ogg"));
            //recitateurs.Add(new Recitateur("Husary", "Murattal", "https://verses.quran.com/Husary/Murattal/ogg/", ".ogg"));
            //recitateurs.Add(new Recitateur("Alafasy", string.Empty, "https://verses.quran.com/Alafasy/ogg/", ".ogg"));
            //recitateurs.Add(new Recitateur("Jibreel", string.Empty, "https://verses.quran.com/Jibreel/mp3/", ".mp3"));
            //recitateurs.Add(new Recitateur("Minshawi", "Mujawwad", "https://verses.quran.com/Minshawi/Mujawwad/ogg/", ".ogg"));
            //recitateurs.Add(new Recitateur("Minshawi", "Murattal", "https://verses.quran.com/Minshawi/Murattal/ogg/", ".ogg"));
            //recitateurs.Add(new Recitateur("Rifai", string.Empty, "https://verses.quran.com/Rifai/ogg/", ".ogg"));
            //recitateurs.Add(new Recitateur("Shatri", string.Empty, "https://verses.quran.com/Shatri/ogg/", ".ogg"));
            //recitateurs.Add(new Recitateur("Shuraym", string.Empty, "https://verses.quran.com/Shuraym/ogg/", ".ogg"));
            //recitateurs.Add(new Recitateur("Sudais", string.Empty, "https://verses.quran.com/Sudais/ogg/", ".ogg"));
            //recitateurs.Add(new Recitateur("Tunaiji", string.Empty, "https://verses.quran.com/Tunaiji/mp3/", ".mp3"));
            File.WriteAllText(@"data\recitateur.json", JsonConvert.SerializeObject(recitateurs, Formatting.Indented));
        }



        internal static void GetQuranFromInternet()
        {
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            string l = "https://api.alquran.cloud/v1/quran/ar.asad";
            string html = wc.DownloadString(l);
            //Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(html);

            //// enlève la bismillah
            //for (int i = 1; i < myDeserializedClass.data.Surahs.Count; i++)
            //{
            //    myDeserializedClass.data.Surahs[i].Ayahs[0].Text = myDeserializedClass.data.Surahs[i].Ayahs[0].Text.Substring(38, myDeserializedClass.data.Surahs[i].Ayahs[0].Text.Length - 38).Trim();
            //    myDeserializedClass.data.Surahs[i].DownloadedRecitateur = new List<int>();
            //}
            ////Il faut faire attention à pas enlever At Tawba car pas de bismillah!!

            //File.WriteAllText(@"data\quran.json", JsonConvert.SerializeObject(myDeserializedClass.data.Surahs, Formatting.Indented));
        }

        private static string Between(string STR, string FirstString, string LastString)
        {
            string FinalString;
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2 = STR.IndexOf(LastString);
            FinalString = STR.Substring(Pos1, Pos2 - Pos1);
            return FinalString;
        }

        private static List<string> ExtractFromBody(string body, string start, string end)
        {
            List<string> matched = new List<string>();

            int indexStart = 0;
            int indexEnd = 0;

            bool exit = false;
            while (!exit)
            {
                indexStart = body.IndexOf(start);

                if (indexStart != -1)
                {
                    indexEnd = indexStart + body.Substring(indexStart).IndexOf(end);

                    matched.Add(body.Substring(indexStart + start.Length, indexEnd - indexStart - start.Length));

                    body = body.Substring(indexEnd + end.Length);
                }
                else
                {
                    exit = true;
                }
            }

            return matched;
        }
    }
}
