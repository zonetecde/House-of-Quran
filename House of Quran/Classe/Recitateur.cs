using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace House_of_Quran
{
    public class Recitateur
    {
        public Recitateur(string nom, string type, string lien, string extension)
        {
            Nom = nom;
            Bytes = type;
            Lien = lien;
            Extension = extension;
        }

        public string Nom { get; set; }
        public string Bytes { get; set; }
        public string Lien { get; set; }
        public string Extension { get; set; }
    }
}
