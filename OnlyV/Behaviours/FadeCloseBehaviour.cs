using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace OnlyV.Behaviours
{
    public class FadeCloseBehaviour : Behavior<Window>
    {
        public static TimeSpan FadeTime { get; } = TimeSpan.FromMilliseconds(750);

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

                var anim = new DoubleAnimation(1.0, 0.0, FadeTime);
                anim.Completed += (s, _) => window.Close();
                window.BeginAnimation(UIElement.OpacityProperty, anim);
            }
        }
    }
}
