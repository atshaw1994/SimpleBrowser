using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBrowser
{
    internal class Bookmark
    {
        public string? Title { get; set; }
        public string? URL { get; set; }

        public Bookmark(string Title = "", string URL = "")
        {
            this.Title = Title;
            this.URL = URL;
        }
    }
}
