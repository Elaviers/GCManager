using System.Collections.Generic;
using System.IO;

namespace GCManager
{
    class Utility
    {
        public static List<string> FindAllFiles(string path, string searchPattern)
        {
            List<string> result = new List<string>();

            foreach (string file in Directory.GetFiles(path, searchPattern))
            {
                result.Add(file);
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                result.AddRange(FindAllFiles(dir, searchPattern));
            }

            return result;
        }

        public static List<string> FindAllDirectories(string path)
        {
            List<string> result = new List<string>();

            foreach (string dir in Directory.GetDirectories(path))
            {
                result.AddRange(FindAllDirectories(dir));
                result.Add(dir);
            }

            return result;
        }

        public static void CopyDirectory(string sourcePath, string destPath)
        {
            DirectoryInfo src = new DirectoryInfo(sourcePath);
            DirectoryInfo dest = new DirectoryInfo(destPath);

            if (!src.Exists)
                return;

            if (!dest.Exists)
                dest.Create();

            foreach (string filepath in Directory.GetFiles(sourcePath))
            {
                File.Copy(filepath, System.IO.Path.Combine(destPath, System.IO.Path.GetFileName(filepath)), true);
            }

            foreach (DirectoryInfo srcDir in src.GetDirectories())
            {
                CopyDirectory(srcDir.FullName, System.IO.Path.Combine(destPath, srcDir.Name));
            }
        }
    }
}
