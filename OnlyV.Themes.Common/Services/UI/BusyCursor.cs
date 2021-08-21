using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Threading;

namespace OnlyV.Themes.Common.Services.UI
{
    public sealed class BusyCursor : IDisposable
    {
        private Cursor _originalCursor;

        public BusyCursor()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => 
            {
                _originalCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Wait; 
            });
        }

        public void Dispose()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => { Mouse.OverrideCursor = _originalCursor; });
        }
    }
}
