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

        public bool Equals(string Title = "", string URL = "")
        {
            return this.Title == Title && this.URL == URL;
        }

        public override bool Equals(object? other)
        {
            return other is not null && other is Bookmark item && Title == item.Title && URL == item.URL;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
