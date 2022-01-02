using System;

namespace iPrintUtility.Printer
{
    /// <summary>
    /// Some parts of were shamefully but necessarily reversed from the Android app
    /// </summary>
    /// <remarks>
    /// QND
    /// </remarks>
    public class PrinterData
    {
        public static Guid[] WriteUUidGuids = { Guid.Parse("0000AE01-0000-1000-8000-00805F9B34FB"), Guid.Parse("0000FF02-0000-1000-8000-00805F9B34FB"), Guid.Parse("0000AB01-0000-1000-8000-00805F9B34FB") };
        public static Guid[] ServiceUuidGuids = { Guid.Parse("0000AE00-0000-1000-8000-00805F9B34FB"), Guid.Parse("0000FF00-0000-1000-8000-00805F9B34FB"), Guid.Parse("0000AB00-0000-1000-8000-00805F9B34FB") };

        public static byte[] FinishLatticeBytes = { 0x51, 0x78, 0xA6, 0x00, 0x0B, 0x00, 0xAA, 0x55, 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x17, 0x13, 0xFF };
        public static byte[] GetDevIdBytes = { 0x51, 0x78, 0xBB, 0x00, 0x01, 0x00, 0x01, 0x07, 0xFF };
        public static byte[] GetDevInfoBytes = { 0x51, 0x78, 0xA8, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF };
        public static byte[] GetDevStateBytes = { 0x51, 0x78, 0xA3, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF };
        public static byte[] RetractPaperBytes = { 0x51, 0x78, 0xA0, 0x00, 0x02, 0x00, 0x30, 0x00, 0xF9, 0xFF };
        public static byte[] FeedPaperBytes = { 0x51, 0x78, 0xA1, 0x00, 0x02, 0x00, 0x30, 0x00, 0xF9, 0xFF };
        public static byte[] Paper300dpiBytes = { 0x51, 0x78, 0xA1, 0x00, 0x02, 0x00, 0x48, 0x00, 0xF3, 0xFF };
        public static byte[] PrintLatticeBytes = { 0x51, 0x78, 0xA6, 0x00, 0x0B, 0x00, 0xAA, 0x55, 0x17, 0x38, 0x44, 0x5F, 0x5F, 0x5F, 0x44, 0x38, 0x2C, 0xA1, 0xFF };
        public static byte[] PrintImgBytes = { 0x51, 0x78, 0xBE, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF };
        public static byte[] PrintTextBytes = { 0x51, 0x78, 0xBE, 0x00, 0x01, 0x00, 0x01, 0x07, 0xFF };
        public static byte[] Quality1Bytes = { 0x51, 0x78, 0xA4, 0x00, 0x01, 0x00, 0x31, 0x97, 0xFF };
        public static byte[] Quality2Bytes = { 0x51, 0x78, 0xA4, 0x00, 0x01, 0x00, 0x32, 0x9E, 0xFF };
        public static byte[] Quality3Bytes = { 0x51, 0x78, 0xA4, 0x00, 0x01, 0x00, 0x33, 0x99, 0xFF };
        public static byte[] Quality4Bytes = { 0x51, 0x78, 0xA4, 0x00, 0x01, 0x00, 0x34, 0x8C, 0xFF };
        public static byte[] Quality5Bytes = { 0x51, 0x78, 0xA4, 0x00, 0x01, 0x00, 0x35, 0x8B, 0xFF };
        public static byte[] SpeedModerationBytes = { 0x51, 0x78, 0xA4, 0x00, 0x01, 0x00, 0x23, 0xE9, 0xFF };
        public static byte[] SpeedThickBytes = { 0x51, 0x78, 0xA4, 0x00, 0x01, 0x00, 0x25, 0xFB, 0xFF };
        public static byte[] SpeedThinBytes = { 0x51, 0x78, 0xA4, 0x00, 0x01, 0x00, 0x22, 0xEE, 0xFF };
        public static byte[] UpdateDevBytes = { 0x51, 0x78, 0xA9, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF };
        public static byte[] WifiDataBytes = { 0x51, 0x78, 0xAA, 0x00, 0x0A, 0x00, 0x33, 0x36, 0x30, 0x73, 0x75, 0x6E, 0x70, 0x65, 0x6E, 0x67, 0x01, 0xFF, 0x51, 0x78, 0xAB, 0x00, 0x08, 0x00, 0x33, 0x35, 0x38, 0x38, 0x37, 0x33, 0x35, 0x31, 0xA9, 0xFF };

        public static string GetDevType(int i)
        {
            return "XW00" + i;
        }

        public static string GetDevWifiState(int i)
        {
            if (i == 0)
            {
                return "No WiFi/No Setting";
            }
            if (i == 1)
            {
                return "No WiFi Setting";
            }
            if (i == 2)
            {
                return "Network not set";
            }
            return i == 3 ? "Network set Wifi" : "";
        }

        public static string GetDeviceState(string state)
        {
            if (state.Length == 0)
            {
                return "";
            }
            if (state.EndsWith("1"))
            {
                return "No paper.";
            }
            if (state.EndsWith("10"))
            {
                return "Paper slot open.";
            }
            if (state.EndsWith("100"))
            {
                return "Too hot.";
            }
            if (state.EndsWith("1000"))
            {
                return "No power, please charge.";
            }
            return "";
        }

        public static byte[] GetBlackening(string modelNo, int i)
        {
            if (modelNo.Equals("GB01"))
            {
                return Quality3Bytes;
            }
            else if (i == 2)
            {
                if (modelNo.Equals("M01") || modelNo.Equals("XW003") || modelNo.Equals("PR") || modelNo.Equals("JX001"))
                {
                    return Quality3Bytes;
                }
                return Quality2Bytes;
            }
            else if (i == 3)
            {
                if (modelNo.Equals("M01") || modelNo.Equals("XW003") || modelNo.Equals("XW001") || modelNo.Equals("PR") || modelNo.Equals("JX001"))
                {
                    return Quality4Bytes;
                }
                return Quality3Bytes;
            }
            else if (i != 5)
            {
                return Quality3Bytes;
            }
            else
            {
                if (modelNo.Equals("GB01"))
                {
                    return Quality4Bytes;
                }
                return Quality5Bytes;
            }
        }

        /// <summary>
        /// Converts integer to byte array
        /// </summary>
        /// <param name="integer"></param>
        /// <returns></returns>
        private static byte[] IntToByte(int integer)
        {
            var b = BitConverter.GetBytes(integer);
            byte[] bytes = new byte[b.Length];

            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(b[i]);

            return bytes;
        }

        public static byte[] SendWifi(string str, string str2)
        {
            byte[] bytes = new byte[str.Length];
            Array.ConvertAll(bytes, q => Convert.ToByte(str));

            byte[] bytes2 = new byte[str2.Length];
            Array.ConvertAll(bytes2, q => Convert.ToByte(str2));

            byte[] bArr = new byte[(bytes.Length + 8 + bytes2.Length + 8)];
            byte[] intToByte = IntToByte(bytes.Length);
            byte[] intToByte2 = IntToByte(str2.Length);
            bArr[0] = 0x51;
            bArr[1] = 0x78;
            bArr[2] = 0xAA;
            bArr[3] = 0;
            bArr[4] = intToByte[0];
            bArr[5] = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                bArr[i + 6] = bytes[i];
            }
            bArr[bytes.Length + 6] = Utilities.Crc8(bytes);
            bArr[bytes.Length + 7] = 0xFF;
            bArr[bytes.Length + 8] = 0x51;
            bArr[bytes.Length + 9] = 0x78;
            bArr[bytes.Length + 10] = 0xAB;
            bArr[bytes.Length + 11] = 0;
            bArr[bytes.Length + 12] = intToByte2[0];
            bArr[bytes.Length + 13] = 0;
            for (int i2 = 0; i2 < bytes2.Length; i2++)
            {
                bArr[i2 + 14 + bytes.Length] = bytes2[i2];
            }
            bArr[bytes.Length + 14 + bytes2.Length] = Utilities.Crc8(bytes2);
            bArr[bytes.Length + 15 + bytes2.Length] = 0xFF;
            return bArr;
        }

        public static byte[] SendDateToWifi(string str, string str2)
        {
            byte[] bytes = new byte[(str + str2).Length];
            Array.ConvertAll(bytes, q => Convert.ToByte((str + str2)));

            byte[] bArr = new byte[(bytes.Length + 10)];
            byte[] intToByte = IntToByte(bytes.Length + 2);
            bArr[0] = 0x51;
            bArr[1] = 0x78;
            bArr[2] = 0xA5;
            bArr[3] = 0;
            bArr[4] = intToByte[0];
            bArr[5] = 0;
            bArr[6] = 0;
            bArr[7] = 0xA0;
            for (int i = 0; i < bytes.Length; i++)
            {
                bArr[i + 8] = bytes[i];
            }
            bArr[bytes.Length + 8] = Utilities.Crc8(bArr, 6, bytes.Length + 2);
            bArr[bytes.Length + 9] = 0xFF;
            return bArr;
        }

        public static byte[] SendWifiPw(string str)
        {
            byte[] bytes = new byte[str.Length];
            Array.ConvertAll(bytes, q => Convert.ToByte(str));

            byte[] bArr = new byte[(bytes.Length + 8)];
            byte[] intToByte = IntToByte(bytes.Length);
            bArr[0] = 0x51;
            bArr[1] = 0x78;
            bArr[2] = 0xAB;
            bArr[3] = 0;
            bArr[4] = intToByte[0];
            bArr[5] = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                bArr[i + 6] = bytes[i];
            }
            bArr[^2] = Utilities.Crc8(bytes);
            bArr[^1] = 0xFF;
            return bArr;
        }

        public static byte[] WriteDeviceId(string str)
        {
            if (str.Length != 12)
            {
                return null;
            }
            byte[] hexToByteArray = HexToByteArray("00" + str);
            byte[] bArr = new byte[(hexToByteArray.Length + 8)];
            bArr[0] = 0x51;
            bArr[1] = 0x78;
            bArr[2] = 0xBB;
            bArr[3] = 0;
            bArr[4] = 0x07;
            bArr[5] = 0;
            for (int i = 0; i < hexToByteArray.Length; i++)
            {
                bArr[i + 6] = hexToByteArray[i];
            }
            bArr[^2] = Utilities.Crc8(hexToByteArray);
            bArr[^1] = 0xFF;
            return bArr;
        }

        public static byte[] HexToByteArray(string str)
        {
            byte[] bytes = new byte[str.Length / 2];

            for (int i = 0; i < str.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);

            return bytes;
        }

        public static byte[] GetDeviceInfo(string deviceAddress, string userId)
        {
            byte[] bArr = GetDevInfoBytes;
            byte[] sendDateToWifi = SendDateToWifi(deviceAddress.Replace(":", ""), userId);
            byte[] bArr2 = GetDevStateBytes;
            byte[] bArr3 = new byte[(bArr.Length + sendDateToWifi.Length + bArr2.Length)];
            Array.Copy(bArr, 0, bArr3, 0, bArr.Length);
            Array.Copy(sendDateToWifi, 0, bArr3, bArr.Length, sendDateToWifi.Length);
            Array.Copy(bArr2, 0, bArr3, bArr.Length + sendDateToWifi.Length, bArr2.Length);
            return bArr3;
        }
    }
}
