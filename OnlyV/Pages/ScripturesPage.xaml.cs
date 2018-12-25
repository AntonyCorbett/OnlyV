namespace OnlyV.Pages
{
    using System.Windows;
    using System.Windows.Controls;
    using ViewModel;

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

        private bool BringButtonIntoView(ItemsControl itemsControl)
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

        private bool BringVerseButtonIntoView(ItemsControl itemsControl)
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
