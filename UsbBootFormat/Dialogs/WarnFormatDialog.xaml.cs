using CsharpHelpers.Helpers;
using CsharpHelpers.WindowServices;
using System.Windows;

namespace UsbBootFormat.Dialogs
{
    public partial class WarnFormatDialog : Window
    {
        public WarnFormatDialog()
        {
            InitializeComponent();
            imgWarning.Source = ImageHelper.BitmapSourceFromIcon(AppHelper.AssemblyInfo.Icon);
            new WindowSystemMenu(this) { IconRemoved = true };
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
