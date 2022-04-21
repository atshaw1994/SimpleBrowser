using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBrowser
{
    internal class HistoryItem
    {
        public string? Title { get; set; }
        public string? URL { get; set; }
        public DateTime LastVisited { get; set; }

        public HistoryItem(string title, string uRL, DateTime lastVisited)
        {
            Title = title;
            URL = uRL;
            LastVisited = lastVisited;
        }
     }
}
