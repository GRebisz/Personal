using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSListener.Data.Model
{
    public class PodcastXMLData
    {
        public Guid id = Guid.Empty;
        public string fk_podcast = "";
        public string podcastdata = "";
        public string lastbuilddate = "";
    }
}