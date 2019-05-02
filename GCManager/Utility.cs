using System.Collections.Generic;
using System.IO;

namespace GCManager
{
    class Utility
    {

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
                File.Copy(filepath, Path.Combine(destPath, Path.GetFileName(filepath)), true);
            }

            foreach (DirectoryInfo srcDir in src.GetDirectories())
            {
                CopyDirectory(srcDir.FullName, Path.Combine(destPath, srcDir.Name));
            }
        }
    }
}
