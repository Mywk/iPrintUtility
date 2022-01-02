/* Copyright (C) 2021 - Mywk.Net
 * Licensed under the EUPL, Version 1.2
 * You may obtain a copy of the Licence at: https://joinup.ec.europa.eu/community/eupl/og_page/eupl
 * Unless required by applicable law or agreed to in writing, software distributed under the Licence is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace iPrintUtility
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// This property can be used to find out if the current window is using the dark theme
        /// </summary>
        public bool IsDarkTheme { get; set; } = false;

        /// <summary>
        /// This property can be used to force NoBlur
        /// </summary>
        public bool NoBlur { get; set; } = false;

        Mutex mutex = new System.Threading.Mutex(false, "426ab1e1-7f92-4880-9a7f-c361dc92b1f3");

        protected override void OnStartup(StartupEventArgs e)
        {
            // Check if an instance of the application is already running
            try
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("An instance of this application is already running.", "Process Affinity Utility");
                    Application.Current.Shutdown(0);
                }
            }
            catch (Exception) { }

            // Detect if windows is using dark theme and apply theme if necessary
            try
            {
                string appsUseLightTheme = string.Empty;
                RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, RegistryView.Registry64);
                localKey = localKey.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
                if (localKey != null)
                {
                    appsUseLightTheme = localKey.GetValue("AppsUseLightTheme").ToString();
                }

                IsDarkTheme = (Int64.Parse(appsUseLightTheme) == 0);


                if (e.Args.Length > 0)
                {
                    foreach (var arg in e.Args)
                    {
                        if (arg.ToLowerInvariant().Contains("dark"))
                            IsDarkTheme = true;
                        else if (arg.ToLowerInvariant().Contains("light"))
                            IsDarkTheme = false;
                        else if (arg.ToLowerInvariant().Contains("noacryl") || arg.ToLowerInvariant().Contains("noblur"))
                            NoBlur = true;
                    }
                }

                var packUri = "Themes/";
                if (IsDarkTheme)
                {
                    packUri += "Dark.xaml";

                    var darkTheme = Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;

                    Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                    Application.Current.Resources.MergedDictionaries.Add(darkTheme);
                }
            }
            catch (Exception ex) { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (mutex != null)
            {
                mutex.Close();
                mutex = null;
            }

            base.OnExit(e);
        }
    }
}
