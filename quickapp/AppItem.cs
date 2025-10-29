using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;

namespace quickapp
{
    public class AppItem
    {
        public string AppName { get; set; }
        public string AppPath { get; set; }
        public string IconPath { get; set; }
        public int LaunchCount { get; set; } = 0;

        public TextBlock RefTextBlock;

        public bool IsValid() => !string.IsNullOrEmpty(AppPath) && File.Exists(AppPath);
    }
}
