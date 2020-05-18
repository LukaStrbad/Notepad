using System;

namespace NotepadCore.Exceptions
{
    // Nasljeđujemo klasu od klase Exception
    internal class InvalidSaveLocationException : Exception
    {
        // Poruka o neispravnoj lokaciji spremanja
        public new readonly string Message = "Input save location is not valid";
        // Svi ostali članovi klase su naslijeđeni
    }
}