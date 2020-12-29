using System.Windows;
using System.Windows.Controls;

namespace DeveloperTest.Views.Behaviors
{
    public class RichTextBehavior
    {
        public static readonly DependencyProperty RichTextProperty =
          DependencyProperty.RegisterAttached("RichText", typeof(string), typeof(RichTextBehavior),
           new PropertyMetadata(OnChanged));

        public static string GetRichText(DependencyObject dependencyObject)
        {
            return (string)dependencyObject.GetValue(RichTextProperty);
        }

        public static void SetRichText(DependencyObject dependencyObject, string richText)
        {
            dependencyObject.SetValue(RichTextProperty, richText);
        }

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
         ((WebBrowser)d).NavigateToString((string)e.NewValue);
    }
}
