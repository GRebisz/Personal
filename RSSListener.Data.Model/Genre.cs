using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSListener.Data.Model
{
    public class Genre
    {
        public Guid id { get; set; }
        public string GenreName = "";
        public Genre(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            GenreName = name;
        }
    }
}
