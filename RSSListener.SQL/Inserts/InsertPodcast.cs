using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace RSSListener.SQL.Inserts
{
    public class InsertPodcast : RSSListener.SQL.Interface.Inserts
    {
        private SqlConnection conn = null;
        private RSSListener.Data.Model.PodCast podcast = null;
        private const string storedproc = "InsertPodcast";
        public InsertPodcast(SqlConnection connection, RSSListener.Data.Model.PodCast podcast)
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
                this.podcast = podcast;
            }
        }
        public bool writeInformation()
        {
            if (isValid())
            {
                SqlCommand comm = new SqlCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;
                comm.Connection = conn;
                comm.CommandText = storedproc;
                comm.Parameters.AddWithValue("@XMLUrl", podcast.URL);
                int res = -1;
                if(podcast.AutomaticDownload)
                {
                    res = 1;
                }
                else
                {
                    res = 0;
                }
                comm.Parameters.AddWithValue("@DownloadAll", res);
                comm.Parameters.AddWithValue("@Name", podcast.PodcastName);
                comm.ExecuteNonQuery();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool isValid()
        {
            if (podcast == null)
            {
                throw new ArgumentNullException("podcast");
            }
            if (String.IsNullOrWhiteSpace(podcast.URL) || String.IsNullOrWhiteSpace(podcast.PodcastName))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
