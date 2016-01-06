using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSListener.Data.Model
{
    public class Artist
    {
        public Guid id { get; set; }
        public string Artistname = "";
        public bool isdj;
        public Artist(string name, bool isdj)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            Artistname = name;
        }
    }
}
