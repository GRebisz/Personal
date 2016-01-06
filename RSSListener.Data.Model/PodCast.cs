using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace RSSListener.Data.Model
{
    public class PodCast
    {
        public Guid id { get; set; }
        public string URL = "";
        public string PodcastName = "";
        public string podcastpath = "";
        public string fk_genre = "";
        public string DefaultArtistName = "";
        public bool useDefault = false;
        public bool AutomaticDownload = false;
        public List<Show> shows = new List<Show>();
        private bool islocalxml = false;
        public Genre genre = null;
        cLogger logger = null;
        public PodCast(string url, string name,string dnbpath, bool localXML=false, Genre genre=null, bool autodownload=false, cLogger logger=null)
        {
            this.URL = url;
            this.PodcastName = name;
            this.genre = genre;
            this.podcastpath = dnbpath;
            this.logger = logger;
            islocalxml = localXML;
            AutomaticDownload = autodownload;
        }
        
    }
}
