/* Copyright (C) 2021 - Mywk.Net
 * Licensed under the EUPL, Version 1.2
 * You may obtain a copy of the Licence at: https://joinup.ec.europa.eu/community/eupl/og_page/eupl
 * Unless required by applicable law or agreed to in writing, software distributed under the Licence is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * 
 * Code related to Newtonsoft.Json and InTheHand.BluetoothLE is licensed under their respective licenses.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace iPrintUtility.Printer
{
    /// <summary>
    /// Utilities to allow us to print with our amazing 
    /// </summary>
    public class PrintDataUtils
    {
        readonly PrinterInfo printerInfo;

        public PrintDataUtils(PrinterInfo printerInfo)
        {
            this.printerInfo = printerInfo;
        }

        /// <summary>
        /// Converts a bitmap to a set of commands that can be sent to the printer
        /// </summary>
        /// <param name="printJobList"></param>
        /// <returns></returns>
        public List<byte[]> BitmapToData(List<PrintJob> printJobList)
        {
            List<byte[]> commands = new List<byte[]>();

            // Set power
            int power = printerInfo.GetPowerValueByType(printerInfo.SelectedPowerIntensity);
            if (power != 0)
            {
                byte[] cmd = new byte[10];
                cmd[0] = 0x51;
                cmd[1] = 0x78;
                cmd[2] = 0xA2;
                cmd[3] = 0x00;
                cmd[4] = 0x02;
                cmd[5] = 0x00;

                // Length
                byte[] bytes = (byte[])(Array)BitConverter.GetBytes(power);
                cmd[6] = bytes[1];
                cmd[7] = bytes[0];

                cmd[8] = Utilities.Crc8(cmd, 6, 2);
                cmd[9] = 0xFF;
                commands.Add(cmd);
            }

            foreach (var printJob in printJobList)
            {
                // Make a copy of the original bitmap but using a supported format
                var bmp = printJob.Bitmap.Clone(new System.Drawing.Rectangle(0, 0, printJob.Bitmap.Width, printJob.Bitmap.Height), PixelFormat.Format32bppArgb);

                // Converting our bitmap to b/w
                // Thanks to vbocan @stackoverflow for the beautiful piece of code
                {
                    using Graphics gr = Graphics.FromImage(printJob.Bitmap); // SourceImage is a Bitmap object
                    var gray_matrix = new float[][]
                    {
                        new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                        new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                        new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                    };

                    var ia = new System.Drawing.Imaging.ImageAttributes();
                    ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(gray_matrix));
                    ia.SetThreshold((float) 0.8); // Change this threshold as needed

                    var rc = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
                    gr.DrawImage(bmp, rc, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, ia);
                    gr.Flush();
                }

                // Set quality according to the print job
                {
                    byte[] qualityCmd;

                    switch (printJob.Quality)
                    {
                        case PrintJob.PrintQuality.Lowest:
                            qualityCmd = PrinterData.Quality1Bytes;
                            break;
                        case PrintJob.PrintQuality.Low:
                            qualityCmd = PrinterData.Quality2Bytes;
                            break;
                        case PrintJob.PrintQuality.Medium:
                            qualityCmd = PrinterData.Quality3Bytes;
                            break;
                        case PrintJob.PrintQuality.High:
                            qualityCmd = PrinterData.Quality4Bytes;
                            break;
                        case PrintJob.PrintQuality.Highest:
                            qualityCmd = PrinterData.Quality5Bytes;
                            break;
                        default:
                            qualityCmd = PrinterData.Quality3Bytes;
                            break;
                    }

                    commands.Add(qualityCmd);
                }

                // Set print type
                if (printJob.Type == PrintJob.PrintType.Text)
                    commands.Add(PrinterData.PrintTextBytes);
                else if (printJob.Type == PrintJob.PrintType.Image)
                    commands.Add(PrinterData.PrintImgBytes);

                if (printJob.Bitmap != null)
                {
                    for (int y = 0; y < printJob.Bitmap.Height; y++)
                    {
                        int width = printJob.Bitmap.Width;

                        byte[] tempArray = new byte[(width / 8) + 1];

                        int bit = 0;
                        for (int x = 0; x < printJob.Bitmap.Width; x++)
                        {
                            // Yes, I am aware of how inefficient this is, lockbits should be used instead
                            var pixel = printJob.Bitmap.GetPixel(x, y);

                            tempArray[bit / 8] >>= 1;

                            if (pixel.R < 0x80 && pixel.G < 0x80 && pixel.B < 0x80 && pixel.A > 0x80)
                                tempArray[bit / 8] |= 0x80;
                            else
                                tempArray[bit / 8] |= 0;

                            bit += 1;
                        }

                        commands.AddRange(LineToCommands(tempArray, printJob.Type));

                    }

                    // Add some white at the end if necessary
                    if (printJob.AddWhite)
                    {
                        for (int i = 0; i < 2; i++)
                            commands.Add(PrinterData.FeedPaperBytes);
                    }
                }
            }

            return commands; ;
        }

        /// <summary>
        /// Converts a line of the bitmap to a command to print
        /// </summary>
        /// <remarks>
        /// This is not completely converted as I didn't have the time to do it, feel free to improve this.
        /// </remarks>
        /// <param name="bmpPixels"></param>
        /// <param name="printImgSize"></param>
        /// <param name="printType"></param>
        /// <returns></returns>
        private List<byte[]> LineToCommands(byte[] bmpPixels, PrintJob.PrintType printJobType)
        {
            List<byte[]> commands = new List<byte[]>();

            // Add line
            {
                byte[] cmd = new byte[10 + bmpPixels.Length];
                cmd[0] = 0x51;
                cmd[1] = 0x78;
                cmd[2] = 0xA2;
                cmd[3] = 0x00;

                // Length
                byte[] bytes = (byte[])(Array)BitConverter.GetBytes(bmpPixels.Length);
                cmd[4] = bytes[0];
                cmd[5] = bytes[1];

                int pos = 6;
                for (int i = 0; i < bmpPixels.Length; i++)
                {
                    cmd[i + 6] = bmpPixels[i];
                    pos++;
                }

                cmd[pos] = Utilities.Crc8(cmd, 6, bmpPixels.Length);
                cmd[pos + 1] = 0xFF;
                commands.Add(cmd);
            }

            // Feed paper
            {
                byte[] feedPaper = FeedPaper(printJobType);
                commands.Add(feedPaper);
            }

            return commands;

        }

        /// <summary>
        /// Feed paper data array
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public byte[] FeedPaper(PrintJob.PrintType printJobType)
        {
            var feedSpeed = printerInfo.TextPrintSpeed;
            if (printJobType == PrintJob.PrintType.Image)
            {
                if (printerInfo.ImgPrintSpeed != 0)
                    feedSpeed = printerInfo.ImgPrintSpeed;
            }

            byte[] bArr = new byte[9];
            bArr[0] = 0x51;
            bArr[1] = 0x78;
            bArr[2] = 0xBD;
            bArr[3] = 0x00;
            bArr[4] = 0x01;
            bArr[5] = 0x00;
            bArr[6] = Convert.ToByte(feedSpeed);
            bArr[7] = Utilities.Crc8(bArr, 6, 1);
            bArr[8] = 0xFF;
            return bArr;
        }

    }
}
