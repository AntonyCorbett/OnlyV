namespace OnlyV.Services.DragDrop
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    internal interface IDragDropService
    {
        event EventHandler EpubFileListChanged;

        bool CanAcceptDrop(DragEventArgs e);

        IReadOnlyCollection<string> GetDroppedFiles(DragEventArgs e);
    }
}
