using Microsoft.Win32;

namespace DerbyApp
{
    internal class RaceResults
    {
        public static readonly HeatList ThirteenCarsFourLanes =
            new HeatList(13, new int[][]
            {
                new int[] { 6, 10, 11, 2 },
                new int[] { 11, 4, 12, 0 },
                new int[] { 8, 0, 6, 9 },
                new int[] { 4, 2, 7, 8 },
                new int[] { 9, 11, 1, 7 },
                new int[] { 1, 6, 5, 4 },
                new int[] { 7, 12, 3, 6 },
                new int[] { 2, 5, 9, 12 },
                new int[] { 3, 9, 4, 10 },
                new int[] { 10, 7, 0, 5 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 12, 8, 10, 1 },
                new int[] { 5, 3, 8, 11 },
            });

        public static void StoreRaceStatus(string raceName, bool inProgress)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DerbyApp");
            key.SetValue("raceName", raceName);
            key.SetValue("inProgress", inProgress);
            key.Close();
        }

        public static bool GetRaceStatus(out string raceName)
        {
            raceName = "";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DerbyApp");
            if (key != null)
            {
                if (key.GetValue("raceName") is string s1) raceName = s1;
                if (key.GetValue("inProgress") is string s2) return bool.Parse(s2);
            }
            return false;
        }
    }
}
