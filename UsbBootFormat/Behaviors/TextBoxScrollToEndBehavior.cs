using System.Windows.Controls;
using System.Windows.Interactivity;

namespace UsbBootFormat.Behaviors
{
    public class TextBoxScrollToEndBehavior : Behavior<TextBox>
    {
        private TextBox _textBox;

        protected override void OnAttached()
        {
            base.OnAttached();

            _textBox = AssociatedObject;
            _textBox.TextChanged += TextBox_TextChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (_textBox != null)
            {
                _textBox.TextChanged -= TextBox_TextChanged;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs ee)
        {
            _textBox.ScrollToEnd();
        }
    }
}
