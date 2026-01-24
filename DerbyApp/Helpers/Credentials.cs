using DerbyApp.Windows;
using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using System;
using System.IO;
using System.Linq;

namespace DerbyApp.Helpers
{
    public class Credentials
    {
        public string DatabaseUsername;
        public string DatabasePassword;

        public Credentials()
        {
            try
            {
                var pib = new PasswordInputBox();
                pib.ShowDialog();
                var compositeKey = new CompositeKey();
                compositeKey.AddUserKey(new KcpPassword(pib.Password));
                var database = new PwDatabase();
                database.Open(new IOConnectionInfo { Path = "KeePassDatabase.kdbx" }, compositeKey, null);
                var matchingEntries = from entry in database.RootGroup.GetEntries(true)
                                      where entry.Strings.ReadSafe("Title") == "Retool"
                                      select entry;
                if (matchingEntries.Any())
                {
                    DatabaseUsername = matchingEntries.First().Strings.ReadSafe("UserName");
                    DatabasePassword= matchingEntries.First().Strings.ReadSafe("Password");
                }

                database.Close();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: Database file not found at {ex.FileName}");
            }
            catch (InvalidCompositeKeyException)
            {
                Console.WriteLine("Error: Invalid master password or key file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
