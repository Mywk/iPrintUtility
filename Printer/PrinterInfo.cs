using System.Collections.Generic;

namespace iPrintUtility.Printer
{
    /// <summary>
    /// Holds all supported printers information
    /// Some parts were shamefully but necessarily reversed from the Android app
    /// </summary>
    /// <remarks>
    /// I couldn't find a lot of those in the market but since they are included in the iPrinter app I'm also including them here for future support
    /// Some stuff here is unused, feel free to expand on it.
    /// </remarks>
    public class PrinterInfo
    {
        public PrinterInfo(string str)
        {
            if (str == "XW001")
            {
                Initialize("XW001", 0, 2, 384, 384, 8, "PR20-", false, 200, 45, 35, 0, 0, false, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "XW002")
            {
                Initialize("XW002", 0, 2, 576, 576, 12, "PR30-", false, 300, 45, 35, 0, 0, true, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "XW004")
            {
                Initialize("XW004", 1, 2, 576, 576, 12, "PRM30-", false, 300, 45, 35, 0, 0, true, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "XW005")
            {
                Initialize("XW005", 1, 3, 384, 384, 8, "", false, 200, 45, 45, 0, 0, true, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "XW006")
            {
                Initialize("XW006", 1, 3, 576, 576, 12, "", false, 300, 45, 35, 0, 0, true, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "XW007")
            {
                Initialize("XW007", 1, 4, 384, 384, 8, "", true, 200, 45, 45, 183, 183, true, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "XW008")
            {
                Initialize("XW008", 1, 4, 576, 576, 12, "", true, 300, 45, 35, 183, 183, true, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "XW009")
            {
                Initialize("XW009", 0, 2, 384, 384, 8, "P25-", false, 200, 45, 45, 0, 0, true, 2, 2, true, 0, 0, 0, false);
            }
            else if (str == "XW009_BLE")
            {
                Initialize("XW009_BLE", 0, 4, 832, 832, 8, "P25-", true, 300, 45, 45, 183, 183, true, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "JX001")
            {
                Initialize("JX001", 0, 2, 384, 384, 8, "JX01-", false, 200, 90, 45, 0, 0, true, 2, 3, false, 0, 0, 0, false);
            }
            else if (str == "JX002_BLE")
            {
                Initialize("JX002", 0, 2, 576, 576, 12, "JX02-", true, 300, 30, 25, 183, 163, false, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "JX003_BLE")
            {
                Initialize("JX003_BLE", 0, 4, 832, 832, 16, "JX03-", true, 200, 20, 20, 183, 183, true, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "JX004_BLE")
            {
                Initialize("JX004", 0, 4, 1280, 1280, 24, "JX04-", true, 300, 60, 20, 183, 183, true, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "JX005_BLE")
            {
                Initialize("JX005_BLE", 0, 8, 1600, 1728, 8, "JX05-", true, 200, 90, 20, 183, 183, true, 3, 2, false, 0, 0, 0, false);
            }
            else if (str == "JX006")
            {
                Initialize("JX006", 0, 2, 384, 384, 8, "JX06-", false, 300, 30, 20, 0, 0, true, 2, 2, true, 0, 0, 0, false);
            }
            else if (str == "XW003")
            {
                Initialize("XW003", 1, 2, 384, 384, 8, "PR25-", false, 200, 45, 25, 0, 0, true, 2, 5, false, 0, 0, 0, false);
            }
            else if (str == "M01")
            {
                Initialize("M01", 1, 2, 384, 384, 8, "M01-", false, 200, 45, 25, 0, 0, true, 2, 5, false, 0, 0, 0, false);
            }
            else if (str == "PR07")
            {
                Initialize("PR07", 1, 2, 384, 384, 8, "PR07-", false, 200, 45, 25, 0, 0, true, 2, 5, false, 0, 0, 0, false);
            }
            else if (str == "PR02")
            {
                Initialize("PR02", 0, 2, 384, 384, 8, "PR02-", true, 200, 10, 10, 123, 123, true, 2, 6, false, 5500, 5500, 5500, true);
            }
            else if (str == "GB01")
            {
                Initialize("GB01", 0, 2, 384, 384, 8, "GB01-", true, 200, 35, 25, 63, 43, false, 4, 6, false, 8000, 12000, 17500, false);
            }
            else if (str == "GB02(BLE)" || str == "GB02")
            {
                Initialize("GB02", 0, 2, 384, 384, 8, "GB02-", true, 200, 26, 25, 83, 83, false, 4, 6, false, 8600, 12000, 16000, false);
            }
            else if (str == "GB03(BLE)" || str == "GB03")
            {
                Initialize("GB03", 0, 2, 384, 384, 8, "GB03-", true, 200, 26, 25, 83, 103, false, 4, 6, false, 8600, 12000, 16000, false);
            }
            else if (str == "GB04(BLE)" || str == "GB04")
            {
                Initialize("GB04", 0, 2, 384, 384, 8, "GB04-", true, 200, 26, 25, 83, 83, false, 4, 6, false, 8600, 12000, 16000, false);
            }
            else if (str == "LY01")
            {
                Initialize("LY01", 0, 2, 384, 384, 8, "LY01-", false, 200, 40, 25, 0, 0, true, 2, 4, false, 0, 20000, 20000, false);
            }
            else if (str == "LY02")
            {
                Initialize("LY02", 0, 2, 384, 384, 8, "LY02-", false, 200, 45, 35, 0, 0, true, 2, 2, false, 0, 0, 0, false);
            }
            else if (str == "LY03")
            {
                Initialize("LY03", 0, 2, 384, 384, 8, "LY03-", true, 200, 10, 10, 83, 83, true, 2, 6, false, 7500, 7500, 7500, true);
            }
            else if (str == "LY10")
            {
                Initialize("LY10", 0, 2, 384, 384, 8, "LY10-", false, 200, 26, 25, 83, 83, true, 2, 4, false, 0, 0, 0, true);
            }
            else if (str == "AI01")
            {
                Initialize("AI01", 1, 2, 384, 384, 8, "AI01-", false, 200, 45, 25, 0, 0, true, 2, 5, false, 0, 0, 0, false);
            }
            else if (str == "GT01")
            {
                Initialize("GT01", 0, 2, 384, 384, 8, "GT01-", true, 200, 26, 25, 83, 83, true, 2, 4, false, 12000, 12000, 12000, true);
            }
            else
                Initialize("", 0, 2, 384, 384, 8, "", false, 200, 45, 45, 0, 0, true, 2, 2, false, 0, 0, 0, false);
        }


        private void Initialize(string modelName, int model, int size, int paperSize, int printSize, int oneLength, string headName, bool canChangeMtu, int devDpi, int imgPrintSpeed, int textPrintSpeed, int imgMtu, int textMtu, bool newCompression, int paperNum, int interval, bool useSPP, int lowEnergy, int mediumEnergy, int highEnergy, bool hadId)
        {
            this.ModelName = modelName;
            this.Model = model;
            this.Size = size;
            this.PaperSize = paperSize;
            this.PrintSize = printSize;
            this.OneLength = oneLength;
            this.HeadName = headName;
            this.CanChangeMtu = canChangeMtu;
            this.DevDpi = devDpi;
            this.ImgPrintSpeed = imgPrintSpeed;
            this.TextPrintSpeed = textPrintSpeed;
            this.ImgMtu = imgMtu;
            this.TextMtu = textMtu;
            this.NewCompression = newCompression;
            this.PaperNum = paperNum;
            this.Interval = interval;
            this.UseSpp = useSPP;
            this.LowPower = lowEnergy;
            this.MediumPower = mediumEnergy;
            this.HighPower = highEnergy;
            this.HasId = hadId;

            SelectedPowerIntensity = PowerIntensity.Medium;
        }

        public static List<int> GetLYEnergyList()
        {
            var arrayList = new List<int>();
            const int valueOf = 5000;
            arrayList.Add(valueOf);
            arrayList.Add(valueOf);
            arrayList.Add(7000);
            arrayList.Add(7000);
            arrayList.Add(9500);
            arrayList.Add(9500);
            arrayList.Add(12000);
            arrayList.Add(12000);
            arrayList.Add(15000);
            arrayList.Add(15000);
            const int  valueOf2 = 18000;
            arrayList.Add(valueOf2);
            arrayList.Add(valueOf2);
            arrayList.Add(20000);
            arrayList.Add(20000);
            return arrayList;
        }

        public bool CanChangeMtu { get; private set; }
        public int DevDpi { get; private set; }
        public bool HasId { get; private set; }
        public string HeadName { get; private set; }
        public int ImgMtu { get; private set; }
        public int ImgPrintSpeed { get; private set; }
        public int Interval { get; private set; }
        public int Model { get; private set; }
        public string ModelName { get; private set; }
        public bool NewCompression { get; private set; }
        public int OneLength { get; private set; }
        public int PaperNum { get; private set; }
        public int PaperSize { get; private set; }
        public int PrintSize { get; private set; }
        public int Size { get; private set; }
        public int TextMtu { get; private set; }
        public int TextPrintSpeed { get; private set; }
        public bool UseSpp { get; private set; }

        public enum PowerIntensity
        {
            Low = 2,
            Medium = 3,
            High = 5
        }

        public int LowPower { get; private set; }
        public int HighPower { get; private set; }
        public int MediumPower { get; private set; }

        public PrinterInfo.PowerIntensity SelectedPowerIntensity { get; set; }

        public int GetPowerValueByType(PowerIntensity power)
        {
            return power switch
            {
                PowerIntensity.Low => LowPower,
                PowerIntensity.Medium => MediumPower,
                PowerIntensity.High => HighPower,
                _ => MediumPower,
            };
        }
        
    }

}
