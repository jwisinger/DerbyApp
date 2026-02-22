using Microsoft.Win32;

namespace DerbyApp.Helpers
{
    public static class DatabaseRegistry
    {
        public static void StoreDatabaseRegistry(string database, string activeRace, string outputFolderName, bool? timeBasedScoring, int? maxRaceTime, string qrCodeLink, string qrPrinterName, string licensePrinterName, string password)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DerbyApp");
            if (database != null) key.SetValue("database", database);
            if (activeRace != null) key.SetValue("activeRace", activeRace);
            if (outputFolderName != null) key.SetValue("outputFolderName", outputFolderName);
            if (timeBasedScoring != null) key.SetValue("timeBasedScoring", timeBasedScoring);
            if (maxRaceTime != null) key.SetValue("maxRaceTime", maxRaceTime);
            if (qrCodeLink != null) key.SetValue("qrCodeLink", qrCodeLink);
            if (qrPrinterName != null) key.SetValue("qrPrinterName", qrPrinterName);
            if (licensePrinterName != null) key.SetValue("licensePrinterName", licensePrinterName);
            if (password != null) key.SetValue("password", password);
            key.Close();
        }

        public static bool GetDatabaseRegistry(out string database, out string activeRace, out string outputFolderName, out bool timeBasedScoring, out int maxRaceTime, out string qrCodeLink, out string qrPrinterName, out string licensePrinterName, out string password)
        {
            database = "";
            activeRace = "";
            outputFolderName = "";
            qrCodeLink = "";
            qrPrinterName = "";
            licensePrinterName = "";
            password = "";
            timeBasedScoring = false;
            maxRaceTime = 10;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DerbyApp");
            if (key != null)
            {
                if (key.GetValue("database") is string s1) database = s1;
                if (key.GetValue("activeRace") is string s2) activeRace = s2;
                if (key.GetValue("outputFolderName") is string s3) outputFolderName = s3;
                if (key.GetValue("timeBasedScoring") is string s4) if (bool.TryParse(s4, out bool b)) timeBasedScoring = b;
                if (key.GetValue("maxRaceTime") is int s5) maxRaceTime = s5;
                if (key.GetValue("qrCodeLink") is string s6) qrCodeLink = s6;
                if (key.GetValue("qrPrinterName") is string s7) qrPrinterName = s7;
                if (key.GetValue("licensePrinterName") is string s8) licensePrinterName = s8;
                if (key.GetValue("password") is string s9) password = s9;
                return true;
            }
            return false;
        }
    }
}
