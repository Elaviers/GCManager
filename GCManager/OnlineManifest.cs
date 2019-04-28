using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCManager
{
    public class OnlineManifest
    {
        public string
            name,
            full_name,
            owner,
            package_url;

        public bool is_pinned;

        public VersionManifest[] versions;

        public class VersionManifest
        {
            public string
                name,
                full_name,
                description,
                icon,
                version_number,
                download_url,
                data_created,
                website_url;

            public int downloads;

            public string[] dependencies;
        }
    }
}
