using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NotepadCore
{
    class InvalidSaveLocation : Exception
    {
        public new readonly string Message = "Input save location is not valid";

        public InvalidSaveLocation() { }

        public InvalidSaveLocation(string message) : base(message) { }
    }
}
