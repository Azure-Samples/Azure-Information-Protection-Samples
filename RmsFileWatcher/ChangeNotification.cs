using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmsFileWatcher
{
    public class ChangeNotification
    {
        public ChangeNotification(string path)
        {
            FullPath = path;
            ChangeTime = DateTime.Now;
            Processed = false;
        }

        public string FullPath { get; set; }
        public DateTime ChangeTime { get; set; }
        public bool Processed { get; set; }
    };
}
