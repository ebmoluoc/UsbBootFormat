using CsharpHelpers.DeviceServices;
using CsharpHelpers.Helpers;
using CsharpHelpers.Interops;
using CsharpHelpers.WindowServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using UsbBootFormat.Services;

namespace UsbBootFormat.ViewModels
{

    using DriveList = List<string>;


    public class MainViewModel : NotifyModel
    {

        private static readonly char[] _InvalidFatVolumeLabelChars = PathHelper.GetInvalidFatVolumeLabelChars();
        private static readonly string _ScriptFile = AppHelper.DataDirectory.GetFilePath("ScriptFile.txt");
        private static readonly Dispatcher _uiDispatcher = Application.Current.Dispatcher;

        private readonly IDialogService _dialogService;
        private Process _process;
        private string _currentData;


        public MainViewModel() : this(new DialogService(), new ArgumentService())
        {
        }


        public MainViewModel(IDialogService dialogService, IArgumentService argumentService)
        {
            FormatCommand = new DelegateCommand<Window>(FormatAction);
            _dialogService = dialogService;

            UpdateDrives();

            var drive = argumentService.Drive;
            if (drive != null)
                SelectedDrive = drive;
        }


        public DelegateCommand<Window> FormatCommand { get; }


        public string WindowTitle
        {
            get { return AppHelper.AssemblyInfo.Title; }
        }


        private DriveList _drives;
        public DriveList Drives
        {
            get { return _drives; }
            private set
            {
                var drive = SelectedDrive;
                SetProperty(ref _drives, value);

                if (_drives.Count == 0)
                {
                    drive = null;
                    OutputText = "\nWaiting for a USB flash drive to be inserted...";
                }
                else if (!_drives.Contains(drive))
                {
                    drive = _drives[0];
                }

                SelectedDrive = drive;
                RaisePropertyChanged(nameof(DrivesEnabled));
            }
        }


        private string _selectedDrive;
        public string SelectedDrive
        {
            get { return _selectedDrive; }
            set
            {
                if (!Drives.Contains(value))
                    value = null;

                SetProperty(ref _selectedDrive, value);
                DiskNumber = GetDiskNumber(_selectedDrive);
                VolumeLabel = GetVolumeLabel(_selectedDrive);
            }
        }


        private string _volumeLabel;
        public string VolumeLabel
        {
            get { return _volumeLabel; }
            set
            {
                if (value == null || value.IndexOfAny(_InvalidFatVolumeLabelChars) == -1)
                    _volumeLabel = value;

                RaisePropertyChanged(nameof(VolumeLabel));

                if (DiskNumber != -1)
                    OutputText = $"\nDiskpart Script\n---------------\n\n{GetScript()}";
            }
        }


        private int _diskNumber;
        public int DiskNumber
        {
            get { return _diskNumber; }
            private set
            {
                SetProperty(ref _diskNumber, value);
                RaisePropertyChanged(nameof(FormatEnabled));

                if (Drives.Count != 0 && _diskNumber == -1)
                    OutputText = "\nThe disk number for this drive could not be retrieved.";
            }
        }


        public bool DrivesEnabled
        {
            get { return ProcessRunning == false && Drives.Count > 1; }
        }


        public bool FormatEnabled
        {
            get { return ProcessRunning == false && DiskNumber != -1; }
        }


        private bool _processRunning;
        public bool ProcessRunning
        {
            get { return _processRunning; }
            private set
            {
                SetProperty(ref _processRunning, value);
                RaisePropertyChanged(nameof(DrivesEnabled));
                RaisePropertyChanged(nameof(FormatEnabled));
            }
        }


        private string _outputText;
        public string OutputText
        {
            get { return _outputText; }
            private set { SetProperty(ref _outputText, value); }
        }


        public void UpdateDrives()
        {
            using (var sdd = new StorageDeviceDescriptor())
            {
                var drives = new DriveList();

                foreach (var drive in Directory.GetLogicalDrives())
                {
                    var deviceName = GetDeviceName(drive);
                    if (sdd.Load(deviceName))
                    {
                        if (sdd.BusType == STORAGE_BUS_TYPE.BusTypeUsb && sdd.RemovableMedia == true)
                        {
                            drives.Add(drive);
                        }
                    }
                }

                Drives = drives;
            }
        }


        private string GetScript()
        {
            return $"select disk {DiskNumber}\nclean\ncreate partition primary\nformat fs=fat32 label=\"{VolumeLabel}\" quick\nactive";
        }


        private void FormatAction(Window window)
        {
            if (_dialogService.WarnFormat(window, $"Format USB Drive ({SelectedDrive})"))
            {
                OutputText = "";

                try
                {
                    File.WriteAllText(_ScriptFile, GetScript());
                }
                catch (Exception ex)
                {
                    AppHelper.Logger.Write(ex.ToString());
                    OutputText = $"\nAn error occurred while writing the script file to disk.\n\n{_ScriptFile}\n\n{ex.Message}";
                    return;
                }

                try
                {
                    StartProcess();
                }
                catch (Exception ex)
                {
                    AppHelper.Logger.Write(ex.ToString());
                    OutputText = $"\nAn error occurred while starting the diskpart process.\n\n{ex.Message}";
                    return;
                }
            }
        }


        private void StartProcess()
        {
            var proc = new Process();

            proc.StartInfo.FileName = "diskpart.exe";
            proc.StartInfo.Arguments = $"/s \"{_ScriptFile}\"";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding(850);
            proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.GetEncoding(850);
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.OutputDataReceived += OutputDataReceived;
            proc.ErrorDataReceived += OutputDataReceived;
            proc.EnableRaisingEvents = true;
            proc.Exited += Exited;

            try
            {
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                _process = proc;
                ProcessRunning = true;
            }
            catch (Exception)
            {
                proc.Dispose();
                throw;
            }
        }


        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var data = e.Data;

            if (data != null && data != _currentData)
            {
                _currentData = data;

                _uiDispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate ()
                {
                    OutputText += $"{data}\n";
                });
            }
        }


        private void Exited(object sender, EventArgs e)
        {
            var exitCode = _process.ExitCode;
            var exitMessage = "Format operation succeeded";

            if (exitCode != 0)
            {
                exitMessage = $"Format operation failed ({exitCode})";
                AppHelper.Logger.Write($"{exitMessage}: {OutputText}");
            }

            _uiDispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                OutputText += $"\n{exitMessage}.";
                ProcessRunning = false;
            });

            _process.Dispose();
            _process = null;
        }


        private static int GetDiskNumber(string drive)
        {
            var diskNumber = -1;
            var deviceName = GetDeviceName(drive);
            if (deviceName != null)
            {
                using (var vde = new VolumeDiskExtents())
                {
                    if (vde.Load(deviceName))
                    {
                        var diskExtents = vde.GetDiskExtents();
                        if (diskExtents.Count == 1)
                        {
                            diskNumber = diskExtents[0].DiskNumber;
                        }
                    }
                }
            }

            return diskNumber;
        }


        private static string GetDeviceName(string drive)
        {
            if (PathHelper.IsRootedPath(drive))
                return $"\\\\.\\{drive.Substring(0, 2)}";

            return null;
        }


        private static string GetVolumeLabel(string drive)
        {
            foreach (var drv in DriveInfo.GetDrives())
                if (drv.IsReady && drv.Name == drive)
                    return drv.VolumeLabel;

            return null;
        }

    }

}
