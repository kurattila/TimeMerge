using System;
using System.Text;

namespace TimeMerge.Utils
{
    internal class NotesContentFormatter
    {
        internal static string Prefix
        {
            get { return "Poznámky:\t"; }
        }

        public static string NotesWithIndent(int indentTabsCount, string multiLineNotes)
        {
            if (string.IsNullOrWhiteSpace(multiLineNotes))
                return null;

            var indentedNotes = new StringBuilder();

            var notesLines = multiLineNotes.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            bool firstLineDone = false;
            foreach (string oneLineFromNote in notesLines)
            {
                if (firstLineDone)
                    indentedNotes.AppendLine();

                for (int tabsCount = 0; tabsCount < indentTabsCount; ++tabsCount)
                    indentedNotes.Append("\t");

                if (!firstLineDone)
                    indentedNotes.Append("Poznámky:\t");
                else
                    indentedNotes.Append("\t\t");

                indentedNotes.Append(oneLineFromNote);
                firstLineDone = true;
            }

            return indentedNotes.ToString();
        }
    }
}
