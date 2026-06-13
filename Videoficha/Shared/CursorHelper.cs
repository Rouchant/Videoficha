using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using System.Reflection;

namespace Videoficha.Shared
{
    public static class CursorHelper
    {
        public static readonly DependencyProperty CursorProperty =
            DependencyProperty.RegisterAttached("Cursor", typeof(InputSystemCursorShape), typeof(CursorHelper), new PropertyMetadata(InputSystemCursorShape.Arrow, OnCursorChanged));

        public static InputSystemCursorShape GetCursor(DependencyObject obj) => (InputSystemCursorShape)obj.GetValue(CursorProperty);
        public static void SetCursor(DependencyObject obj, InputSystemCursorShape value) => obj.SetValue(CursorProperty, value);

        private static void OnCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && e.NewValue is InputSystemCursorShape shape)
            {
                var cursor = InputSystemCursor.Create(shape);
                
                // Set the ProtectedCursor property using reflection
                var prop = typeof(UIElement).GetProperty("ProtectedCursor", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (prop != null)
                {
                    prop.SetValue(element, cursor);
                }
            }
        }
    }
}
