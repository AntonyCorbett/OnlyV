namespace OnlyV.Behaviours
{
    using System;
    using System.Windows;
    using System.Windows.Interactivity;
    using System.Windows.Media.Animation;

    public class FadeCloseBehaviour : Behavior<Window>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Closing += OnAssociatedObjectClosing;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Closing -= OnAssociatedObjectClosing;
        }

        private void OnAssociatedObjectClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is Window window)
            {
                window.Closing -= OnAssociatedObjectClosing;
                e.Cancel = true;

                var anim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(0.75));
                anim.Completed += (s, _) => window.Close();
                window.BeginAnimation(UIElement.OpacityProperty, anim);
            }
        }
    }
}
