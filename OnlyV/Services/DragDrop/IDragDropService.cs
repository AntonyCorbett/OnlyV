using System;
using System.Collections.Generic;
using System.Windows;

namespace OnlyV.Services.DragDrop
{
    internal interface IDragDropService
    {
        event EventHandler EpubFileListChanged;

        bool CanAcceptDrop(DragEventArgs e);

        IReadOnlyCollection<string> GetDroppedFiles(DragEventArgs e);
    }
}
