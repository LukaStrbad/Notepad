using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using System.Windows.Documents;
using NotepadCore.Annotations;

namespace NotepadCore.ExtensionMethods
{
    public static class TextPointerExtensions
    {
        /// <summary>
        ///     Gets actual TextPointer position that includes FlowDocument tags
        /// </summary>
        /// <param name="from">Starting TextPointer position</param>
        /// <param name="offset">Offset from <param name="from"></param></param>
        public static TextPointer GetTextPointerAtOffset(this TextPointer from, int offset)
        {
            var ret = from.GetPositionAtOffset(0);
            var i = 0;

            while (i < offset)
            {
                if (ret.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    i++;

                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
                if (ret?.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    return ret;
            }

            return ret;
        }

        public static int GetOffsetAtTextPointer(this TextPointer from, TextPointer to)
        {
            int offset = 0;
            var pointer = from.GetPositionAtOffset(0);

            for (int i = 0; i < from.GetOffsetToPosition(to); i++)
            {
                if (pointer == null)
                    break;
                pointer = pointer.GetPositionAtOffset(1);

                if (pointer?.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    offset++;
            }

            return offset;
        }

    }
}
