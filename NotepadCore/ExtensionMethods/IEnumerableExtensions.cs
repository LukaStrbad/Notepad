using System;
using System.Collections.Generic;
using System.Linq;

namespace NotepadCore.ExtensionMethods
{
    public static class IEnumerableExtensions
    {
        // Dodatna metoda koja vraća različite elemente kolekcije korištenjem  
        // selektora
        public static IEnumerable<TSource> Distinct<TSource, TResult>(this IEnumerable<TSource> enumerable,
            Func<TSource, TResult> selector)
        {
            // HashSet je struktura koja sprema elemente i može istog trena 
            // saznati postoji li neki element
            // Objekt se inicijalizira sa početnom veličinom radi brzine
            var exists = new HashSet<TResult>(enumerable.Count());

            // Program prolazi kroz svaki element trenutne kolekcije
            foreach (var element in enumerable)
            {
                // Metoda Add vraća true ako je element uspješno dodan u HashSet
                // Ako nije to znači da se element već pojavio u kolekciji pa ga 
                // nije potrebno vratiti
                if (exists.Add(selector(element)))
                {
                    // Ako je element dodan vraćamo ga
                    yield return element;
                }
            }
        }
    }
}