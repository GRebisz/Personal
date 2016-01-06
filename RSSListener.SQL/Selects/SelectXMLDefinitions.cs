using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using RSSListener.Data.Model;

namespace RSSListener.SQL.Selects
{
    public class SelectXMLDefinitions : RSSListener.SQL.Interface.Selects<XMLDefinition>
    {
        private SqlConnection conn = null;
        private cLogger logger = null;
        private Management manager = null;
        private List<XMLDefinition> xmldefinitions = null;
        private const string storedproc = "SelectXMLDefinition";
        public SelectXMLDefinitions(SqlConnection connection, cLogger logger, Management mngr)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            else if (connection.State != System.Data.ConnectionState.Open)
            {
                throw new Exception("connection's state is not open");
            }
            else
            {
                conn = connection;
                manager = mngr;
                this.logger = logger;
            }
        }
        public List<XMLDefinition> getAll()
        {
            List<XMLDefinition> xmldefs = new List<XMLDefinition>();
            SqlCommand comm = new SqlCommand();
            comm.Connection = conn;
            comm.CommandText = storedproc;
            comm.CommandType = System.Data.CommandType.StoredProcedure;
            SqlDataReader dr = comm.ExecuteReader();
            if (dr != null)
            {
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        XMLDefinition xmldef = new XMLDefinition();
                        xmldef.id = Guid.Parse(dr["id"].ToString());
                        xmldef.fk_podcast = dr["fk_podcast"].ToString();
                        xmldef.xpathToItems = dr["xpathToItems"].ToString();
                        xmldef.xpathToLastBuildDate = dr["xpathToLastBuild"].ToString();
                        xmldef.DescriptionTag = dr["DescriptionTag"].ToString();
                        xmldef.EnclosureTag = dr["EnclosureTag"].ToString();
                        xmldef.TitleTag = dr["TitleTag"].ToString();
                        xmldef.PublishedTag = dr["PublishedTag"].ToString();
                        xmldef.titleruleguid = dr["fk_titlerule"].ToString();
                        xmldef.descriptionruleguid = dr["fk_descriptionrule"].ToString();
                        xmldef.publishedruleguid = dr["fk_publishedRule"].ToString();
                        xmldef.urlruleguid = dr["fk_urlrule"].ToString();
                        xmldef.enclosureruleguid = dr["fk_enclosurerule"].ToString();
                        xmldef.artistruleguid = dr["fk_ArtistRule"].ToString();
                        xmldefs.Add(xmldef);
                    }
                }
                if (!dr.IsClosed)
                {
                    dr.Close();
                    dr.Dispose();
                }
                foreach (XMLDefinition x in xmldefs)
                {
                    if (!String.IsNullOrWhiteSpace(x.titleruleguid))
                    {
                        Rule2 titlerule = manager.getRule(Guid.Parse(x.titleruleguid));
                        x.TitleRule = titlerule;
                    }
                    if (!string.IsNullOrWhiteSpace(x.publishedruleguid))
                    {
                        Rule2 publishedrule = manager.getRule(Guid.Parse(x.publishedruleguid));
                        x.PublishedRule = publishedrule;
                    }
                    if (!string.IsNullOrWhiteSpace(x.urlruleguid))
                    {
                        Rule2 urlrule = manager.getRule(Guid.Parse(x.urlruleguid));
                        x.URLRule = urlrule;
                    }
                    if (!string.IsNullOrWhiteSpace(x.enclosureruleguid))
                    {
                        Rule2 enclosurerule = manager.getRule(Guid.Parse(x.enclosureruleguid));
                        x.EnclosureRule = enclosurerule;
                    }
                    if (!string.IsNullOrWhiteSpace(x.descriptionruleguid))
                    {
                        Rule2 descriptionrule = manager.getRule(Guid.Parse(x.descriptionruleguid));
                        x.DescriptionRule = descriptionrule;
                    }
                    if (!string.IsNullOrWhiteSpace(x.artistruleguid))
                    {
                        Rule2 artistrule = manager.getRule(Guid.Parse(x.artistruleguid));
                        x.ArtistRule = artistrule;
                    }
                }
            }
            xmldefinitions = xmldefs;
            return xmldefs;
        }

        public XMLDefinition getByID(Guid id)
        {
            List<XMLDefinition> xmldeflist = null;
            if (xmldefinitions == null)
            {
                xmldeflist = getAll();
            }
            else
            {
                xmldeflist = xmldefinitions;
            }
            foreach (XMLDefinition a in xmldeflist)
            {
                if (a.id == id)
                {
                    return a;
                }
            }
            return null;
        }

        public XMLDefinition getByPodcast(PodCast p)
        {
            List<XMLDefinition> xmldeflist = null;
            if (xmldefinitions == null)
            {
                xmldeflist = getAll();
            }
            else
            {
                xmldeflist = xmldefinitions;
            }
            foreach (XMLDefinition x in xmldeflist)
            {
                if (Guid.Parse(x.fk_podcast) == p.id)
                {
                    return x;
                }
            }
            return null;
        }

        public XMLDefinition getByName(string name)
        {
            throw new Exception("Deprecated");
        }
    }
}
