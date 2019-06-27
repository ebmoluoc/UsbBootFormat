using System.Windows;
using UsbBootFormat.Dialogs;

namespace UsbBootFormat.Services
{

    public interface IDialogService
    {
        bool WarnFormat(Window owner, string title);
    }


    public sealed class DialogService : IDialogService
    {

        public bool WarnFormat(Window owner, string title)
        {
            var dialog = new WarnFormatDialog { Owner = owner, Title = title };
            return dialog.ShowDialog() == true;
        }

    }

}
