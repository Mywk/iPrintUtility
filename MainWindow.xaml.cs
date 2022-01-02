/* Copyright (C) 2021 - Mywk.Net
 * Licensed under the EUPL, Version 1.2
 * You may obtain a copy of the Licence at: https://joinup.ec.europa.eu/community/eupl/og_page/eupl
 * Unless required by applicable law or agreed to in writing, software distributed under the Licence is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * 
 * Code related to Newtonsoft.Json and InTheHand.BluetoothLE is licensed under their respective licenses.
 */
using InTheHand.Bluetooth;
using iPrintUtility.Printer;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Formatting = Newtonsoft.Json.Formatting;
using Image = System.Drawing.Image;

namespace iPrintUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AcrylicWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool _isLoaded = false;

        /// <summary>
        /// Load previous scan if available or scan if necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            BottomLabel.Content = "iPrint Utility  v" + version.Major + "." + version.Minor + " © " + DateTime.Now.Year + " - Mywk.Net";

            if (CheckForUpdates())
                UpdateLabel.Visibility = Visibility.Visible;

            // Check if WebView2 is present before continuing
            var ver = CoreWebView2Environment.GetAvailableBrowserVersionString();
            if (String.IsNullOrEmpty(ver))
            {
                if (MessageBox.Show("One of the required dependencies could not be found.\r\n\r\nWould you like to download it from the Microsoft website?", "Error", MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes)
                {
                    var targetURL = "https://developer.microsoft.com/en-us/microsoft-edge/webview2/#download-section";
                    var psi = new ProcessStartInfo
                    {
                        FileName = targetURL,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }

                this.Close();
                return;
            }

            bool devicesLoaded = false;

            // If a previous scan occurred we attempt to load the scanned devices
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ScannedDevices))
            {
                try
                {
                    BluetoothPrinter deviceToSelect = null;

                    var devices = JsonDeserialize(Properties.Settings.Default.ScannedDevices);
                    foreach (var device in devices)
                    {
                        bool skip = false;
                        if (device.LocalDevice == null)
                        {
                            BluetoothDevice dev = await BluetoothDevice.FromIdAsync(device.Id);

                            if (dev == null || device.LocalDevice == null)
                                skip = true;
                            else
                                device.LocalDevice = dev;
                        }

                        if (!skip)
                        {
                            DevicesComboBox.Items.Add(device);

                            if (deviceToSelect == null && device.LocalDevice.Name.Contains("GB01"))
                                deviceToSelect = device;
                        }
                    }

                    if (deviceToSelect != null)
                        DevicesComboBox.SelectedItem = deviceToSelect;

                    if (DevicesComboBox.Items.Count > 0)
                    {
                        devicesLoaded = true;
                        AddLog("Loaded previously scanned devices.");
                    }
                }
                catch (Exception ex)
                {
                    AddLog("An error occurred attempting to load previously scanned devices: " + ex.Message);
                }
            }

            // Load last selected quality
            QualityComboBox.SelectedIndex = Properties.Settings.Default.Quality;

            // Otherwise perform a fresh scan
            if (!devicesLoaded)
                _ = RefreshAvailableDevices();

            await LoadEditorAsync();

            MainGrid.IsEnabled = true;

            // Hotfix for Windows making the window not correctly render
            this.Height += 1;
            this.Width += 1;

            _isLoaded = true;
        }

        /// <summary>
        /// Self explanatory
        /// </summary>
        private bool isPrinting = false;

        /// <summary>
        /// Check if a newer version of the software is available
        /// </summary>
        private bool CheckForUpdates()
        {
            try
            {
                var web = new System.Net.WebClient();
                var url = "https://Mywk.Net/software.php?assembly=" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                var responseString = web.DownloadString(url);

                foreach (var str in responseString.Split('\n'))
                {
                    if (str.Contains("Version"))
                    {
                        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                        if (version.Major + "." + version.Minor != str.Split('=')[1])
                            return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Loads our beautiful chromium browser and the editor into it
        /// </summary>
        /// <returns></returns>
        private async Task LoadEditorAsync()
        {
            var htmlContent = LoadResource("Editor.editor.html");

            var quillJs = LoadResource("Editor.quill.js");
            var quillImageDropJs= LoadResource("Editor.quill.image.drop.js");
            var quillImageResizeJs = LoadResource("Editor.quill.image.resize.js");
            var quillStyle = LoadResource("Editor.quill.snow.css");

            htmlContent = htmlContent.Replace("quill_js_core", quillJs);
            htmlContent = htmlContent.Replace("quill_js_module_image_drop", quillImageDropJs);
            htmlContent = htmlContent.Replace("quill_js_module_image_resize", quillImageResizeJs);
            htmlContent = htmlContent.Replace("quill_style", quillStyle);

            var env = await CoreWebView2Environment.CreateAsync(null, Path.Combine(Path.GetTempPath(),"iPrinter"));
            await WebView.EnsureCoreWebView2Async(env);
            WebView.NavigateToString(htmlContent);
        }

        BluetoothPrinter selectedDevice;

        private void ConnectedUiToggle(bool set)
        {
            PrintButton.IsEnabled = FeedButton.IsEnabled = RetractButton.IsEnabled = set;
        }

        private void SelectedDeviceGattServerDisconnectedEvent(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ConnectButton.IsEnabled = true;
                ConnectedUiToggle(false);
                ConnectButton.Content = "Connect";
            }));
        }


        /// <summary>
        /// Deserializes a list of BluetoothLocalDevice objects
        /// </summary>
        /// <param name="toDeserialize"></param>
        /// <returns></returns>
        private List<BluetoothPrinter> JsonDeserialize(string toDeserialize)
        {
            return JsonConvert.DeserializeObject<List<BluetoothPrinter>>(toDeserialize);
        }

        /// <summary>
        /// Serializes a list of BluetoothLocalDevice objects
        /// </summary>
        /// <param name="toSerialize"></param>
        /// <returns></returns>
        private string JsonSerialize(List<BluetoothPrinter> toSerialize)
        {
            return JsonConvert.SerializeObject(toSerialize, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                        });
        }

        /// <summary>
        /// Adds text to the log TextBox and scrolls to end afterwards
        /// </summary>
        /// <param name="text"></param>
        void AddLog(string text)
        {
            const string newLine = "\r\n";

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LogTextBox.Text += (LogTextBox.Text.Length == 0 ? "" : newLine + new String('-', 45) + newLine) + text;
                LogTextBox.ScrollToEnd();
            }));

        }

        bool isScanning = false;

        /// <summary>
        /// Scans for devices and updates the UI accordingly
        /// </summary>
        /// <returns></returns>
        async Task RefreshAvailableDevices()
        {
            if (isScanning)
                return;

            isScanning = true;

            RefreshButton.IsEnabled = false;
            DevicesComboBox.Items.Clear();
            AddLog("Scanning devices.\r\nThis may take up to 30 seconds.");


            var devices = await BluetoothPrinter.ScanDevicesAsync();

            foreach (var device in devices)
            {
                DevicesComboBox.Items.Add(device);
                AddLog("Compatible device found:\r\n" + device.ToString()); ;
            }

            AddLog("Scan complete.");

            if (DevicesComboBox.Items.Count > 0)
            {
                DevicesComboBox.SelectedIndex = 0;

                AddLog("Found " + DevicesComboBox.Items.Count + " devices.");

                var serialized = JsonSerialize(devices);
                Properties.Settings.Default.ScannedDevices = serialized;
                Properties.Settings.Default.Save();
            }
            else
                AddLog("No devices found.");

            RefreshButton.IsEnabled = true;
            isScanning = false;
        }


        /// <summary>
        /// Utility method for converting a byte array to string
        /// </summary>
        /// <param name="ba"></param>
        /// <returns></returns>
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }


        private string filename = "";
        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPrinting)
            {
                selectedDevice.CancelSend();
                PrintButton.Content = "Print";
                isPrinting = false;
                return;
            }

            isPrinting = true;
            FeedButton.IsEnabled = RetractButton.IsEnabled = false;

            PrintButton.Content = "Cancel";

            ProgressBar.Value = 0;
            ProgressBar.Foreground = System.Windows.Media.Brushes.Yellow;

            var bitmap = await GenerateScreenshotAsync();

            PrinterInfo printerInfo = new PrinterInfo(selectedDevice.LocalDevice.Name);
            PrintDataUtils printDataUtils = new PrintDataUtils(printerInfo);
            List<PrintJob> list = new List<PrintJob>();

            var printJob = new PrintJob
            {
                Bitmap = bitmap,
                Type = PrintJob.PrintType.Image,
                Quality = (PrintJob.PrintQuality)QualityComboBox.SelectedIndex,
                AddWhite = true // Add some empty lines at the end so we can cut the paper with the content in it
            };
            list.Add(printJob);

            var data = printDataUtils.BitmapToData(list);

            ProgressBar.Maximum = data.Count;

            var sent = await selectedDevice.SendRange(data);

            ProgressBar.Value = ProgressBar.Maximum;
            ProgressBar.Foreground = sent ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;

            PrintButton.Content = "Print";
            FeedButton.IsEnabled = RetractButton.IsEnabled = true;

            isPrinting = false;
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            _ = RefreshAvailableDevices();
        }

        private void devicesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConnectButton.IsEnabled = false;
            ConnectedUiToggle(false);

            if (DevicesComboBox.SelectedIndex != -1)
            {
                selectedDevice = (BluetoothPrinter)DevicesComboBox.SelectedItem;
                selectedDevice.AddLogEvent += SelectedDevice_AddLogEvent;
                selectedDevice.ReportProgressEvent += SelectedDeviceOnReportProgressEvent;
                ConnectButton.IsEnabled = true;
            }
        }

        private void SelectedDeviceOnReportProgressEvent(object? sender, EventArgs e)
        {
            ProgressBar.Value++;
        }

        private void SelectedDevice_AddLogEvent(object sender, EventArgs e)
        {
            AddLog(((BluetoothPrinter.LogStringEventArgs)e).Log);
        }

        private bool isConnecting = false;
        private bool isConnected = false;

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (isConnecting)
                return;

            isConnecting = true;

            DevicesComboBox.IsEnabled = false;
            RefreshButton.IsEnabled = false;
            ConnectButton.IsEnabled = false;

            if (selectedDevice != null)
            {
                if (!isConnected)
                {
                    if(!selectedDevice.IsConnected())
                    {
                        if (!await selectedDevice.Connect())
                        {
                            AddLog("Unable to connect.");
                        }
                    }

                    if (selectedDevice.IsConnected())
                    {
                        selectedDevice.GattServerDisconnectedEvent += SelectedDeviceGattServerDisconnectedEvent;
                        AddLog("Device connected.");
                        ConnectButton.Content = "Disconnect";
                        ConnectedUiToggle(true);
                        isConnected = true;
                    }
                }
                else
                {
                    selectedDevice.Disconnect();
                    isConnected = false;
                    AddLog("Device disconnected.");
                }
            }

            ConnectButton.IsEnabled = true;
            isConnecting = false;
        }

        private static string LoadResource(string fileName)
        {
            string ret;

            string resourceFileName = "iPrintUtility." + fileName;

            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourceFileName);

            if (stream == null)
                throw new FileNotFoundException("Cannot find mappings file.", resourceFileName);

            using (StreamReader reader = new StreamReader(stream))
                ret = reader.ReadToEnd();

            return ret;
        }

        public Image Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        /// <summary>
        /// Generates a screenshot from the webview
        /// </summary>
        /// <remarks>
        /// QND but works well
        /// </remarks>
        /// <returns></returns>
        public async Task<Bitmap> GenerateScreenshotAsync()
        {
            InfoLabel.Content = "Preparing print..";

            var tempHeight = WebView.ActualHeight;

            WebView.Margin = new Thickness(0, 500, 0, 0);

            await WebView.ExecuteScriptAsync("togglePrint(true)");

            var contentHeight = await WebView.ExecuteScriptAsync("getPrintHeight()");

            string rawLayoutMetrics = await WebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.getLayoutMetrics", "{}"); ;
            JObject o3 = JObject.Parse(rawLayoutMetrics);

            JToken dataHeight = o3["contentSize"]["height"];
            WebView.Height = dataHeight.Value<int>();

            JToken dataWidth = o3["contentSize"]["width"];

            await Task.Delay(200);

            JObject parameters = JObject.Parse("{\"Viewport\": { " +
                                               "\"x\": 0," +
                                               "\"y\": 1," +
                                               "\"height\":" + dataHeight.Value<int>() + "," +
                                               "\"width\":" + dataWidth.Value<int>() +
                                               "}}");
            string r3 = await WebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.captureScreenshot", parameters.ToString());
            o3 = JObject.Parse(r3);
            JToken data = o3["data"];
            string dataStr = data.ToString();

            Image image = Base64ToImage(dataStr);

            await Task.Delay(200);

            if (editorShown)
                await WebView.ExecuteScriptAsync("togglePrint(false)");

            WebView.Margin = new Thickness(0);
            WebView.Height = tempHeight;

            // Crop the image accordingly
            Bitmap bmpImage = new Bitmap(image);
            return bmpImage.Clone(new Rectangle(0, 0, image.Width, int.Parse(contentHeight)), bmpImage.PixelFormat);
        }

        /// <summary>
        /// No need for zoom changing in our webview is there? :)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebView_OnZoomFactorChanged(object? sender, EventArgs e)
        {
            WebView.ZoomFactor = 1;
        }

        /// <summary>
        /// Move window from anywhere
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }


        private void ClearLogButton_OnClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.Text = "";
        }

        private void BottomLabel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var targetURL = "https://mywk.net/software/iprint-utility";
            var psi = new ProcessStartInfo
            {
                FileName = targetURL,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void UpdateLabel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BottomLabel_OnMouseLeftButtonDown(sender, e);
        }

        private bool editorShown = true;

        private async void ToggleEditButton_OnClick(object sender, RoutedEventArgs e)
        {
            editorShown = !editorShown;
            await WebView.ExecuteScriptAsync("togglePrint(" + (!editorShown ? "true" : "false") + ")");
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (isPrinting)
            {
                if (MessageBox.Show("You are currently printing, are you sure you want to stop and exit?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            
            if(isConnected && selectedDevice != null)
                selectedDevice.Disconnect();
        }

        private void QualityComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded)
                return;

            Properties.Settings.Default.Quality = QualityComboBox.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private async void FeedButton_OnClick(object sender, RoutedEventArgs e)
        {
            FeedButton.IsEnabled = false;

            await selectedDevice.Send(PrinterData.FeedPaperBytes);

            if (isConnected)
                FeedButton.IsEnabled = true;
        }

        private async void RetractButton_OnClick(object sender, RoutedEventArgs e)
        {
            RetractButton.IsEnabled = false;

            await selectedDevice.Send(PrinterData.RetractPaperBytes);

            if (isConnected)
                RetractButton.IsEnabled = true;
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                e.Handled = true;
            }
        }
    }
}
