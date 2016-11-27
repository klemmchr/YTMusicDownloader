using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

        private static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
#if DEBUG
            ShowWindow(handle, SW_SHOW);
#else
            ShowWindow(handle, SW_HIDE);
#endif

#if DEBUG
            Console.ReadKey();
#endif
        }
    }
}
