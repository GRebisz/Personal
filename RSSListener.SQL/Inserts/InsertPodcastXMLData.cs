using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace RSSListener.SQL.Inserts
{
    public class InsertPodcastXMLData : RSSListener.SQL.Interface.Inserts
    {
        private SqlConnection conn = null;
        public RSSListener.Data.Model.PodcastXMLData xmldataitem = null;
        private const string storedproc = "insertPodCastXML";
        public InsertPodcastXMLData(SqlConnection connection, RSSListener.Data.Model.PodcastXMLData item)
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
                this.xmldataitem = item;
            }
        }
        public bool writeInformation()
        {
            bool endres = false;
            if (isValid())
            {
                SqlCommand comm = new SqlCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;
                comm.Connection = conn;
                comm.CommandText = storedproc;
                comm.Parameters.Add(new SqlParameter("@FK_Podcast", xmldataitem.fk_podcast));
                comm.Parameters.Add(new SqlParameter("@podcastdata", xmldataitem.podcastdata));
                comm.Parameters.Add(new SqlParameter("lastbuilddate", xmldataitem.lastbuilddate));
                SqlDataReader dr = comm.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    string n = dr[0].ToString();
                    if (n != "101")
                    {
                        endres = true;
                    }
                }
                if (!dr.IsClosed)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
            else
            {
                endres = false;
            }
            return endres;
        }

        public bool isValid()
        {
            if (xmldataitem.id == Guid.NewGuid() || String.IsNullOrWhiteSpace(xmldataitem.lastbuilddate) || String.IsNullOrWhiteSpace(xmldataitem.podcastdata))
            {
                return false;
            }
            return true;
        }
    }
}
