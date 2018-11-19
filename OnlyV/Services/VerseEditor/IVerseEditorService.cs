namespace OnlyV.Services.VerseEditor
{
    internal interface IVerseEditorService
    {
        void Add(
            string epubPath, 
            int bookNumber, 
            int chapter, 
            int verse, 
            string modifiedText);

        void Delete(
            string epubPath,
            int bookNumber,
            int chapter,
            int verse);

        string Get(
            string epubPath,
            int bookNumber,
            int chapter,
            int verse);
    }
}
