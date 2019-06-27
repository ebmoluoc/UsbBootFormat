using CsharpHelpers.Helpers;
using System;

namespace UsbBootFormat.Services
{

    public interface IArgumentService
    {
        string Drive { get; }
    }


    public class ArgumentService : IArgumentService
    {

        private readonly string[] _args = Environment.GetCommandLineArgs();


        public string Drive
        {
            get { return EnvironmentHelper.GetArgument("-d:", _args)?.ToUpper(); }
        }

    }

}
