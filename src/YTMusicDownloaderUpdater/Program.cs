using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

// ReSharper disable InconsistentNaming

namespace YTMusicDownloaderUpdater
{
    public class Program
    {
        #region External

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

#if DEBUG
        private const int SW_SHOW = 5;
#else
        private const int SW_HIDE = 0;
#endif

        #endregion

        #region Fields

        private static string _targetDirectory;
        private static string _zipPath;
        private static string _appPath;

        #endregion

        private static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
#if DEBUG
            ShowWindow(handle, SW_SHOW);
#else
            ShowWindow(handle, SW_HIDE);
#endif
            if (args.Length < 3)
                return;

            _targetDirectory = args[0];
            _zipPath = args[1];
            _appPath = args[2];

            Console.WriteLine($"Target dir: {_targetDirectory}");
            Console.WriteLine($"Zip path: {_zipPath}");
            Console.WriteLine($"App path: {_appPath}");

            Thread.Sleep(1000);

            try
            {
                PerformUpdate(_zipPath, _targetDirectory);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Console.WriteLine(ex);
            }
            finally
            {
#if DEBUG
                Console.ReadKey();
#endif
                UpdatingEngine.OpenBaseApp(_appPath);
            }
        }
        
        private static void PerformUpdate(string zipPath, string targetDir)
        {
            if (!UpdatingEngine.CheckForZip(zipPath))
                throw new InvalidOperationException();

            UpdatingEngine.CleanupDirectory(targetDir);
            UpdatingEngine.ExtractAsset(zipPath, targetDir);
        }
    }
}