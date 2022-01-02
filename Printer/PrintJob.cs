/* Copyright (C) 2021 - Mywk.Net
 * Licensed under the EUPL, Version 1.2
 * You may obtain a copy of the Licence at: https://joinup.ec.europa.eu/community/eupl/og_page/eupl
 * Unless required by applicable law or agreed to in writing, software distributed under the Licence is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * 
 * Code related to Newtonsoft.Json and InTheHand.BluetoothLE is licensed under their respective licenses.
 */
using System.Drawing;

namespace iPrintUtility.Printer
{
    /// <summary>
    /// All the data required for a print job
    /// </summary>
    public class PrintJob
    {
        public bool AddWhite { get; set; } = false;
        public Bitmap Bitmap { get; set; }
        public PrintType Type { get; set; }
        public PrintQuality Quality { get; set; }

        public enum PrintQuality
        {
            Lowest = 0,
            Low,
            Medium,
            High,
            Highest
        }

        public enum PrintType
        {
            Text = 0,
            Image = 1
        }

    }
}
