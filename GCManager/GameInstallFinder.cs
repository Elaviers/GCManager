using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace GCManager
{
    class GameInstallFinder
    {
        private static KeyValuePair<string, string> ReadVDFKeyValue(string line)
        {
            string[] arr = new string[2];
            int tokenIndex = 0;

            for (int ci = 0; ci < line.Length; ci++)
            {
                if (line[ci] == '"')
                {
                    bool lastCharWasEscape = false;
                    char c;

                    while (++ci < line.Length && (c = line[ci]) != '"')
                    {
                        if (lastCharWasEscape && c == '\\')
                            continue;

                        arr[tokenIndex] += c;
                        lastCharWasEscape = c == '\\';
                    }

                    tokenIndex++;

                    if (tokenIndex == 2)
                        break;
                }
            }

            return new KeyValuePair<string, string>(arr[0], arr[1]);
        }
        public static string FindGameInstallDirectory()
        {
            //Find Game Install

            string steamDir = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "InstallPath", null);

            if (steamDir != null)
            {
                string steamAppsDir = Path.Combine(steamDir, "steamapps");


                string libraryFoldersVDFData = File.ReadAllText(Path.Combine(steamAppsDir, "libraryfolders.vdf"));
                string[] lines = libraryFoldersVDFData.Split('\n', '\r');
                int currentLine = 0;

                string libraryDir = Path.Combine(steamAppsDir, "common");

                do
                {
                    if (File.Exists(Path.Combine(libraryDir, "Risk of Rain 2", "Risk of Rain 2.exe")))
                        return Path.Combine(libraryDir, "Risk of Rain 2");

                    //Find next library dir
                    libraryDir = null;

                    KeyValuePair<string, string> kv;
                    while (currentLine < lines.Length)
                    {
                        kv = ReadVDFKeyValue(lines[currentLine++]);

                        int unused;

                        if (int.TryParse(kv.Key, out unused))
                        {
                            libraryDir = Path.Combine(kv.Value, "steamapps", "common");
                            break;
                        }
                    }

                } while (libraryDir != null);
            }

            MessageBoxResult result = MessageBox.Show("Your Risk of Rain 2 install directory couldn't be found.\nDo you want to provide it manually or not?", "Are you a pirate or is this program just bad?", MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return null;

            return FindInstallDir_Dialog();
        }

        public static string FindInstallDir_Dialog()
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Select you Risk of Rain 2 executable";
            ofd.Filter = "Risk Of Rain 2 Executable|Risk Of Rain 2.exe";
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == true)
                return Path.GetDirectoryName(ofd.FileName);

            return null;
        }
    }
}
