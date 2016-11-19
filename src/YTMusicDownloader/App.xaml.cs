/*
    Copyright 2016 Christian Klemm

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NLog;

namespace YTMusicDownloader
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly List<string> _arguments = new List<string>();

#if !DEBUG
        private readonly Mutex _mutex;
#endif

        public App()
        {
#if !DEBUG
            try
            {
                Mutex mutex;
                if (Mutex.TryOpenExisting("YtMusicDownloader", out mutex))
                    Environment.Exit(0);

                _mutex = new Mutex(false, "YTMusicDownloader");
            }
            catch
            {
                Environment.Exit(0);
            }
#endif

            ParseCommandLineArgs();

            // Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            // Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

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

                MessageBox.Show(
                    "A critical error occured.\nYou can find a crash dump in your installation folder\nFurthermore a crash report has already been send automaticially if your internet connection has been established.",
                    "Error", MessageBoxButton.OK);

                Thread.Sleep(5000);
                Environment.Exit(1);
            }
        }
    }
}