using System;

namespace NotepadCore
{
    internal class InvalidSaveLocation : Exception
    {
        public new readonly string Message = "Input save location is not valid";

        public InvalidSaveLocation()
        {
        }

        public InvalidSaveLocation(string message) : base(message)
        {
        }
    }
}