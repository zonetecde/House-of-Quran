using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace House_of_Quran
{
    internal class Sourate
    {
        // Position de la sourate
        public int Id { get; set; }

        // Nom de la sourate en arabe
        public string Name { get; set; }

        // Translitération du nom de la sourate
        public string Transliteration { get; set; }

        // Mecquoise / Medinoise
        public string Type { get; set; }

        // Nombre de verset
        public int TotalVerses { get; set; }

        // Liste de ses versets
        public List<Verse> Verses { get; set; }
    }

    internal class Verse
    {
        // Position du verset
        public int Id { get; set; }

        // Verset en arabe
        public string Text { get; set; }
    }
}
