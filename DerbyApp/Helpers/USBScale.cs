//Original code source: http://stackoverflow.com/questions/11961412/read-weight-from-a-fairbanks-scb-9000-usb-scale
using HidLibrary;
using System.Threading;
using System;
using System.Linq;

namespace DerbyApp.Helpers
{
    class USBScale
    {
        private HidDevice scale;
        private HidDeviceData inData;

        public bool IsConnected
        {
            get
            {
                return scale != null && scale.IsConnected;
            }
        }

        public decimal ScaleStatus
        {
            get
            {
                return inData.Data[1];
            }
        }

        public decimal ScaleWeightUnits
        {
            get
            {
                return inData.Data[2];
            }
        }

        public static HidDevice[] GetDevices()
        {
            //The next line contains the product/vendor ID numbers for the Dymo 25lb Postal Scale, change these depdending on what scale you're using 
            return [.. HidDevices.Enumerate(0x0922, 0x8003)];
        }

        public bool Connect()
        {
            // Find a Scale
            HidDevice[] deviceList = GetDevices();

            if (deviceList.Length > 0)

                return Connect(deviceList[0]);

            else

                return false;
        }

        public bool Connect(HidDevice device)
        {
            scale = device;
            int waitTries = 0;
            scale.OpenDevice();

            // sometimes the scale is not ready immedietly after
            // Open() so wait till its ready
            while (!scale.IsConnected && waitTries < 10)
            {
                Thread.Sleep(50);
                waitTries++;
            }
            return scale.IsConnected;
        }

        public void Disconnect()
        {
            if (scale.IsConnected)
            {
                scale.CloseDevice();
                scale.Dispose();
            }
        }

        public void DebugScaleData()
        {
            for (int i = 0; i < inData.Data.Length; ++i)
            {
                Console.WriteLine("Byte {0}: {1}", i, inData.Data[i]);
            }
        }

        public decimal GetWeight()
        {
            decimal weight;
            decimal weightInOz = 0;
            bool isStable = false;

            if (scale.IsConnected)
            {
                inData = scale.Read(250);
                // Byte 0 == Report ID?
                // Byte 1 == Scale Status (1 == Fault, 2 == Stable @ 0, 3 == In Motion, 4 == Stable, 5 == Under 0, 6 == Over Weight, 7 == Requires Calibration, 8 == Requires Re-Zeroing)
                // Byte 2 == Weight Unit
                // Byte 3 == Data Scaling (decimal placement)
                // Byte 4 == Weight LSB
                // Byte 5 == Weight MSB

                // FIXME: dividing by 100 probably wont work with
                // every scale, need to figure out what to do with
                // Byte 3
                //weight = (Convert.ToDecimal(inData.Data[4]) + Convert.ToDecimal(inData.Data[5]) * 256) / 10;

                switch (Convert.ToInt16(inData.Data[2]))
                {
                    case 2:  // Scale reading in g
                        weight = (Convert.ToDecimal(inData.Data[4]) + Convert.ToDecimal(inData.Data[5]) * 256);
                        weightInOz = weight * (decimal)0.035274;
                        break;
                    case 11: // Ounces
                        weight = (Convert.ToDecimal(inData.Data[4]) + Convert.ToDecimal(inData.Data[5]) * 256) / 10;
                        weightInOz = weight;
                        break;
                    case 12: // Pounds
                        // already in pounds, do nothing
                        break;
                }
                isStable = (inData.Data[1] == 0x4) || (inData.Data[1] == 0x2);
            }

            if (isStable) return weightInOz;
            else return (decimal)10.0;
        }
    }
}
