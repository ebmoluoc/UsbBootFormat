using CsharpHelpers.Helpers;
using System.Windows;

namespace UsbBootFormat
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppHelper.CatchUnhandledException = true;

            var assemblyInfo = new AssemblyInfo(typeof(App).Assembly, "Resources.LICENSE");
            var dataDirectory = new AppDataRoaming(assemblyInfo.Product);
            var logFilePath = dataDirectory.GetFilePath($"{assemblyInfo.FileName}.log");
            var logger = new TextFileLogger(logFilePath);

            AppHelper.AssemblyInfo = assemblyInfo;
            AppHelper.DataDirectory = dataDirectory;
            AppHelper.Logger = logger;
            AppHelper.SetAppMutex($"Global\\{assemblyInfo.Guid}");
            AppHelper.SetInstanceMutex($"Local\\{assemblyInfo.Guid}-IM", true);
        }
    }
}
