/* Copyright (C) 2021 - Mywk.Net
 * Licensed under the EUPL, Version 1.2
 * You may obtain a copy of the Licence at: https://joinup.ec.europa.eu/community/eupl/og_page/eupl
 * Unless required by applicable law or agreed to in writing, software distributed under the Licence is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * 
 * Code related to Newtonsoft.Json and InTheHand.BluetoothLE is licensed under their respective licenses.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InTheHand.Bluetooth;
using Newtonsoft.Json;

namespace iPrintUtility.Printer
{
    /// <summary>
    /// A sort of interface class that internally uses BluetoothDevice from the InTheHand library
    /// </summary>
    /// <remarks>
    /// In order to prevent a long scan time this class is serialized to a Json string that gets loaded on program startup
    /// </remarks>
    public class BluetoothPrinter
    {
        public static async Task<List<BluetoothPrinter>> ScanDevicesAsync()
        {
            List<BluetoothPrinter> devices = new List<BluetoothPrinter>();

            var scannedDevices = await Bluetooth.ScanForDevicesAsync();

            foreach (var dev in scannedDevices)
            {
                // Don't ask
                try
                {
                    if (dev == null || dev.Name == "")
                        continue;
                }
                catch (Exception)
                {
                    continue;
                }


                var device = new BluetoothPrinter(dev);

                List<string> supportedPrinters = new List<string>() { "MX10", "XW001", "XW002", "XW003", "XW004", "XW005", "XW006", "XW007", "XW008", "XW009", "JX001", "JX002", "JX003", "JX004", "JX005", "JX006", "M01", "PR07", "PR02", "GB01", "GB02", "GB03", "GB04", "LY01", "LY02", "LY03", "LY10", "AI01", "GT01" };
                
                if(supportedPrinters.Any(p => p.Contains(dev.Name)))
                    devices.Add(device);
            }

            return devices;
        }

        [JsonConstructor]
        public BluetoothPrinter() { }

        [JsonIgnore]
        BluetoothDevice localDevice = null;

        [JsonIgnore]
        public BluetoothDevice LocalDevice
        {
            get { return localDevice; }
            set { localDevice = value; }
        }

        /// <summary>
        /// We save the Id on this class to allow saving it via serialization and re-loading the same device without having to scan again
        /// </summary>
        string id = "";
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public BluetoothPrinter(BluetoothDevice dev)
        {

            this.localDevice = dev;
            this.id = dev.Id;

        }

        public event EventHandler AddLogEvent;
        public event EventHandler ReportProgressEvent;
        public event EventHandler GattServerDisconnectedEvent;

        public bool IsConnected()
        {
            return (bool)localDevice.Gatt.IsConnected;
        }

        public class LogStringEventArgs : EventArgs
        {
            public LogStringEventArgs(string str)
            {
                this.Log = str;
            }

            public string Log { get; }
        }

        public class ProgressEventArgs : EventArgs
        {
            public ProgressEventArgs() {}
        }

        /// <summary>
        /// Method for adding whatever logs we want
        /// </summary>
        /// <param name="str"></param>
        private void AddLog(string str)
        {
            if (AddLogEvent != null)
                AddLogEvent?.Invoke(this, new LogStringEventArgs(str));
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return localDevice.Name + "\t{" + localDevice.Id + "}";
        }

        bool isBusy = false;

        /// <summary>
        /// Connects to the device
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Connect()
        {
            bool ret = false;

            try
            {
                await localDevice.Gatt.ConnectAsync();

                if (localDevice.Gatt.IsConnected)
                {
                    localDevice.GattServerDisconnected += GattServerDisconnectedEvent;
                    ret = true;
                }
            }
            catch (Exception e)
            {
                AddLog(e.Message);
            }

            return ret;
        }

        /// <summary>
        /// Disconnects from the device
        /// </summary>
        public void Disconnect()
        {
            localDevice.Gatt.Disconnect();

            GattServerDisconnectedEvent?.Invoke(this, null);
            localDevice.GattServerDisconnected -= GattServerDisconnectedEvent;

            writeCharacteristic = null;
        }

        private byte[] lastResponse = null;

        /// <summary>
        /// Last response from a successful send with reponse
        /// </summary>
        public byte[] LastResponse
        {
            get { return lastResponse; }
        }

        private bool inProgress = false;
        private bool pendingCancel = false;

        /// <summary>
        /// Send data expecting a response, the response will be stored at LastResponse
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Success</returns>
        public async Task<bool> SendWithResponse(byte[] content)
        {
            if (inProgress)
                return false;

            pendingCancel = false;

            return await SendInternal(content, true);
        }


        /// <summary>
        /// Sends a range of commands without expecting a response
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<bool> SendRange(List<byte[]> content)
        {
            if (inProgress)
                return false;

            inProgress = true;

            bool ret = true;
            foreach (var cmd in content)
            {
                if (!await SendInternal(cmd, false))
                {
                    ret = false;
                    break;
                }

                if (pendingCancel)
                {
                    pendingCancel = false;
                    ret = false;
                    break;
                }

                if (ReportProgressEvent != null)
                    ReportProgressEvent?.Invoke(this, new ProgressEventArgs());

                await Task.Delay(1);
            }

            inProgress = false;
            return ret;
        }

        public void CancelSend()
        {
            if(inProgress)
                pendingCancel = true;
        }

        /// <summary>
        /// Send data without expecting a response
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Success</returns>
        public async Task<bool> Send(byte[] content)
        {
            if (inProgress)
                return false;

            pendingCancel = false;

            return await SendInternal(content, false);
        }

        GattCharacteristic writeCharacteristic = null;

        /// <summary>
        /// Sends the message to the device, updates response if required
        /// </summary>
        /// <param name="content"></param>
        /// <param name="withResponse"></param>
        /// <returns></returns>
        private async Task<bool> SendInternal(byte[] content, bool withResponse)
        {
            if (isBusy)
            {
                AddLog("Busy.");
                return false;
            }

            isBusy = true;

            bool ret = false;

            try
            {
                if (localDevice.Gatt.IsConnected)
                {

                    if (writeCharacteristic == null)
                    {
                        var services = await localDevice.Gatt.GetPrimaryServicesAsync();

                        foreach (var service in services)
                        {
                            if (service.IsPrimary)
                            {
                                // Find the correct writeCharacteristic uuid - QND
                                foreach (var item in await service.GetCharacteristicsAsync())
                                {
                                    if (item.Properties == (withResponse
                                        ? (GattCharacteristicProperties.Read | GattCharacteristicProperties.Write)
                                        : GattCharacteristicProperties.WriteWithoutResponse))
                                    {
                                        try
                                        {
                                            if (withResponse)
                                                await item.WriteValueWithResponseAsync(
                                                    (byte[]) (Array) PrinterData.GetDevInfoBytes);
                                            else
                                                await item.WriteValueWithoutResponseAsync(
                                                    (byte[]) (Array) PrinterData.GetDevInfoBytes);

                                            writeCharacteristic = item;
                                            break;
                                        }
                                        catch (Exception e)
                                        {
                                            int x = 0;
                                            x++;
                                        }
                                    }
                                }
                            }

                            if (writeCharacteristic != null)
                                break;
                        }
                    }

                    if (writeCharacteristic != null)
                    {

                        const int maxSize = 256;
                        for (int bytesSent = 0; bytesSent < content.Length; bytesSent += maxSize)
                        {
                            int size = (bytesSent + maxSize > content.Length) ? content.Length - bytesSent : maxSize;
                            byte[] data = new byte[size];
                            Array.Copy(content, data, size);

                            if (withResponse)
                            {
                                // Send request
                                await writeCharacteristic.WriteValueWithResponseAsync(data);

                                // And receive data
                                lastResponse = await writeCharacteristic.ReadValueAsync();
                            }
                            else
                            {
                                // Just send the request
                                await writeCharacteristic.WriteValueWithoutResponseAsync(data);
                            }
                        }

                        ret = true;
                    }
                }
                else
                    writeCharacteristic = null;
            }
            catch (Exception e)
            {
                AddLog(e.Message);
            }

            isBusy = false;
            return ret;
        }
    }
}
