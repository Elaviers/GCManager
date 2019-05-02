using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCManager
{
    public class Version
    {
        public float major, minor, patch;

        public Version(string s)
        {
            string[] tokens = s.Split('.');
            if (tokens.Length >= 3)
            {
                major = float.Parse(tokens[0]);
                minor = float.Parse(tokens[1]);
                patch = float.Parse(tokens[2]);
            }
        }

        public static bool operator <(Version a, Version b)
        {
            if (a.major < b.major)
                return true;

            if (a.minor < b.minor)
                return true;

            if (a.patch < b.patch)
                return true;

            return false;
        }
        public static bool operator >(Version a, Version b)
        {
            if (a.major > b.major)
                return true;

            if (a.major == b.major)
            {
                if (a.minor > b.minor)
                    return true;

                if (a.minor == b.minor && a.patch > b.patch)
                    return true;
            }

            return false;
        }
    }
}
