using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSListener.Data.Model
{
    public class XMLDefinition
    {
        public Guid id = Guid.Empty;
        public PodCast podcast = null;
        public string fk_podcast = "";
        public string xpathToItems = "";
        public string xpathToLastBuildDate = "";
        public string DescriptionTag = "";
        public string EnclosureTag = "";
        public string TitleTag = "";
        public string PublishedTag = "";
        public string URLTag = "";
        public Rule2 TitleRule = null;
        public string titleruleguid = "";
        public Rule2 DescriptionRule = null;
        public string descriptionruleguid = "";
        public Rule2 EnclosureRule = null;
        public string enclosureruleguid = "";
        public Rule2 PublishedRule = null;
        public string publishedruleguid = "";
        public Rule2 URLRule = null;
        public string urlruleguid = "";
        public Rule2 ArtistRule = null;
        public string artistruleguid = "";
    }
}
