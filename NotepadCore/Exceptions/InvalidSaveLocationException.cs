using System;

namespace NotepadCore.Exceptions
{
    internal class InvalidSaveLocationException : Exception
    {
        public new readonly string Message = "Input save location is not valid";

        public InvalidSaveLocationException()
        {
        }
    }
}