using System.Collections.Generic;

namespace NotepadCore.ExtensionMethods
{
    public static class StringExtensions
    {
        // Metoda koja vraća sve indekse gdje se pojavljuje vrijednost value u 
        // trenutnom string-u
        public static IEnumerable<int> IndexesOf(this string str, string value)
        {
            // Ako je vrijednost prazna ili null program izlazi iz metode
            if (string.IsNullOrEmpty(value))
                yield break;
            // Petlja kreće od 0 i povećava se za veličinu vrijednosti
            for (var i = 0;; i += value.Length)
            {
                // Nova vrijednost varijable i će biti indeks vrijednosti
                // Pretraživanje indeksa kreće od i
                i = str.IndexOf(value, i);
                // Ako nema više vrijednosti u string-u program izlazi iz petlje
                if (i == -1)
                    break;
                // Ako je pretraživanje indeksa uspješno, program vraća indeks
                yield return i;
            }
        }
    }
}