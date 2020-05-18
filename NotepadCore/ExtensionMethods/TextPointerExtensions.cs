using System.Windows.Documents;

namespace NotepadCore.ExtensionMethods
{
    public static class TextPointerExtensions
    {
        // Metoda GetPositionAtOffset koja je ugrađena u klasu TextPointer vraća
        // novi TextPointer objekt za određen broj znakova. Ti znakovi uključuju 
        // korisniku nevidljive oznake koje označuju različite dijelove tekst.
        // Ova metoda vraća TextPointer objekt pomaknut za vidljiv broj znakova 
        public static TextPointer GetTextPointerAtOffset(this TextPointer from, int offset)
        {
            // Kopiranje objekta from kako da ga metoda ne promijeni
            var ret = from.GetPositionAtOffset(0);
            // Brojanje znakova kreće od 0
            var i = 0;
            // Dok je trenutni broj znakova manji od traženog
            while (i < offset)
            {
                // Ako je sljedeći znak tekstualnog oblika program povećava broj 
                // znakova za 1
                if (ret.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    i++;

                // Program provjerava je li sljedeći TextPointer null te ako je 
                // izlazi iz metode
                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    return ret;
                // Ako sljedeći TextPointer nije null, program pomiče ret
                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
            }
            // Ako je petlja završena program vraća objekt ret
            return ret;
        }

        // Metoda vraća za koliko je mjesta pomaknut TextPointer objekt od 
        // trenutnog
        public static int GetOffsetAtTextPointer(this TextPointer from, TextPointer to)
        {
            // Broj pomaknutih mjesta jednak je dužini teksta u objektu TextRange
            return new TextRange(from, to).Text.Length;
        }

    }
}