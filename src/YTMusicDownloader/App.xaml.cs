using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using NLog;
using MessageBox = System.Windows.MessageBox;

namespace YTMusicDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly List<string> _arguments = new List<string>();
        private readonly Mutex _mutex;

        public App()
        {
            try
            {
                Mutex mutex;
                if(Mutex.TryOpenExisting("YtMusicDownloader", out mutex))
                    Environment.Exit(0);
            }
            catch
            {
                // ignored
            }

            _mutex = new Mutex(true, "YTMusicDownloader");

            ParseCommandLineArgs();

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var logger = LogManager.GetCurrentClassLogger();

            logger.Info("");
            logger.Info("======================================");
            logger.Info("Application startup: {0}", DateTime.Now);
        }

        private void ParseCommandLineArgs()
        {
            var args = Environment.GetCommandLineArgs();

            for (var index = 1; index < args.Length; index++)
            {
                var arg = args[index].Replace("-", "");
                _arguments.Add(arg);
            }
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MainWindow.Hide();

            LogManager.GetLogger("CrashLog").Fatal(e.Exception);

            if (!_arguments.Contains("debugging"))
            {
                LogManager.GetLogger("CrashWebReport").Fatal(e.Exception);

                MessageBox.Show("A critical error occured.\nYou can find a crash dump in your installation folder\nFurthermore a crash report has already been send automaticially if your internet connection has been established.", "Error", MessageBoxButton.OK);

                Thread.Sleep(5000);
                Environment.Exit(1);
            }
        }
    }
}
