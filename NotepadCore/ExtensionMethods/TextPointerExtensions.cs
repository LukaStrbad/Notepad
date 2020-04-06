using System.Windows.Documents;

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

                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    return ret;
                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
            }

            return ret;
        }

        /// <summary>
        /// Gets offset from <paramref name="from"> to <paramref name="to"> excluding FlowDocument tags
        /// </summary>
        /// <param name="from">This text pointer</param>
        /// <param name="to">End text pointer</param>
        /// <returns>Offset between text pointers</returns>
        public static int GetOffsetAtTextPointer(this TextPointer from, TextPointer to)
        {
            return new TextRange(from, to).Text.Length;
        }

    }
}
