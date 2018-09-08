namespace OnlyV.Services.DragDrop
{
    using System.Collections.Generic;
    using System.Windows;

    internal interface IDragDropService
    {
        bool CanAcceptDrop(DragEventArgs e);

        IReadOnlyCollection<string> GetDroppedFiles(DragEventArgs e);
    }
}
