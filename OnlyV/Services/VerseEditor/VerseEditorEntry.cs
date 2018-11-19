namespace OnlyV.Services.VerseEditor
{
    internal class VerseEditorEntry
    {
        public string EpubPath { get; set; }

        public string BookName { get; set; }

        public int BookNumber { get; set; }

        public int Chapter { get; set; }

        public int Verse { get; set; }

        public string ModifiedVerseText { get; set; }
    }
}
