namespace OnlyV.Pages
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using OnlyV.Helpers;
    using OnlyV.ViewModel;

    /// <summary>
    /// Interaction logic for PreviewPage.xaml
    /// </summary>
    public partial class PreviewPage : UserControl
    {
        public PreviewPage()
        {
            InitializeComponent();

            // for keyboard binding (e.g. F5)
            Focusable = true;
            Loaded += (s, e) => Keyboard.Focus(this);
        }

        private void ImageMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var files = CreateImageFileList();

                var obj = new DataObject(DataFormats.FileDrop, files.ToArray());
                
                DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy);
            }
        }

        private IReadOnlyCollection<string> CreateImageFileList()
        {
            var vm = (PreviewViewModel)DataContext;

            var folder = FileUtils.GetTempOnlyVFolder();
            var destFileOrFolder = vm.SaveImagesToFolder(folder);

            if (Directory.Exists(destFileOrFolder))
            {
                // multiple files...
                var files = Directory.GetFiles(destFileOrFolder).ToList();
                files.Sort();
                return files;
            }
            
            // single image file.
            return new List<string> { destFileOrFolder };
        }
    }
}
