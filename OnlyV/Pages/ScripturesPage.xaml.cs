using System.Windows;
using System.Windows.Controls;
using OnlyV.ViewModel;

namespace OnlyV.Pages
{
    /// <summary>
    /// Interaction logic for ScripturesPage.xaml
    /// </summary>
    public partial class ScripturesPage : UserControl
    {
        public ScripturesPage()
        {
            InitializeComponent();
        }

        private void UserControlLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!BringButtonIntoView(HebrewBooksControl))
            {
                BringButtonIntoView(GreekBooksControl);
            }

            BringButtonIntoView(ChaptersControl);
            BringVerseButtonIntoView(VersesControl);
        }

        private static bool BringButtonIntoView(ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items)
            {
                if (item is ButtonModel bm)
                {
                    if (bm.Selected)
                    {
                        var b = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                        b?.BringIntoView();
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool BringVerseButtonIntoView(ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items)
            {
                if (item is VerseButtonModel bm)
                {
                    if (bm.Selected)
                    {
                        var b = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                        b?.BringIntoView();
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
