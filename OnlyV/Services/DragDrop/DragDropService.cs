namespace OnlyV.Services.DragDrop
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DragDropService : IDragDropService
    {
        public bool CanAcceptDrop(DragEventArgs e)
        {
            if (e.Data is DataObject dataObject && dataObject.ContainsFileDropList())
            {
                foreach (var filePath in dataObject.GetFileDropList())
                {
                    if (IsEpubFile(filePath))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public IReadOnlyCollection<string> GetDroppedFiles(DragEventArgs e)
        {
            var result = new List<string>();

            if (e.Data is DataObject dataObject && dataObject.ContainsFileDropList())
            {
                foreach (var filePath in dataObject.GetFileDropList())
                {
                    if (IsEpubFile(filePath))
                    {
                        result.Add(filePath);
                    }
                }
            }

            return result;
        }

        private bool IsEpubFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            return ext != null && ext.Equals(".epub", StringComparison.OrdinalIgnoreCase);
        }
    }
}
