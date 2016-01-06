using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSListener.Data.Model
{
    public class SubGenre
    {
        public Guid id { get; private set; }
        public string SubGenreName = "";
        public SubGenre(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            SubGenreName = name;
        }
    }
}
