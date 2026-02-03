using DerbyApp.Windows;
using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using System.Linq;

namespace DerbyApp.Helpers
{
    public class Credentials
    {
        public string Password;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string GoogleDriveUsername;
        public string GoogleDrivePassword;
        public string FileUploaderApiKey;

        private int OpenVault(out PwDatabase db)
        {
            try
            {
                var compositeKey = new CompositeKey();
                compositeKey.AddUserKey(new KcpPassword(Password));
                db = new PwDatabase();
                db.Open(new IOConnectionInfo { Path = "KeePassDatabase.kdbx" }, compositeKey, null);
                return 0;
            }
            catch (InvalidCompositeKeyException)
            {
                db = null;
                return -1;
            }
        }

        public Credentials(string password)
        {
            PwDatabase db;
            Password = password;

            while (OpenVault(out db) != 0)
            {
                var pib = new PasswordInputBox();
                pib.ShowDialog();
                Password = pib.Password;
            }

            var matchingEntries = from entry in db.RootGroup.GetEntries(true)
                                  where entry.Strings.ReadSafe("Title") == "Retool"
                                  select entry;
            if (matchingEntries.Any())
            {
                DatabaseUsername = matchingEntries.First().Strings.ReadSafe("UserName");
                DatabasePassword= matchingEntries.First().Strings.ReadSafe("Password");
            }

            matchingEntries = from entry in db.RootGroup.GetEntries(true)
                              where entry.Strings.ReadSafe("Title") == "RetoolFileUploader"
                              select entry;
            if (matchingEntries.Any())
            {
                FileUploaderApiKey = matchingEntries.First().Strings.ReadSafe("Password");
            }

            matchingEntries = from entry in db.RootGroup.GetEntries(true)
                              where entry.Strings.ReadSafe("Title") == "GoogleDrive"
                              select entry;
            if (matchingEntries.Any())
            {
                GoogleDriveUsername = matchingEntries.First().Strings.ReadSafe("UserName");
                GoogleDrivePassword = matchingEntries.First().Strings.ReadSafe("Password");
            }

            db.Close();
        }
    }
}
