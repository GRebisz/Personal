using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using RSSListener.Data.Model;

namespace RSSListener.SQL.Selects
{
    public class SelectPodcast : RSSListener.SQL.Interface.Selects<RSSListener.Data.Model.PodCast>
    {
        private SqlConnection conn = null;
        private List<PodCast> podcasts = null;
        private Management manager = null;
        private const string storedproc = "selectPodcasts";
        private cLogger logger = null;
        public SelectPodcast(SqlConnection connection,Management manager, cLogger logger)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            else if (connection.State != System.Data.ConnectionState.Open)
            {
                throw new Exception("connection's state is not open");
            }
            else if (manager == null)
            {
                throw new Exception("manager element passed is null");
            }
            else
            {
                conn = connection;
                this.manager = manager;
                this.logger = logger;
            }
        }
        public List<Data.Model.PodCast> getAll()
        {
            List<PodCast> resultset = new List<PodCast>();
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
                        PodCast a = new PodCast(dr["XMLUrl"].ToString(), dr["Name"].ToString(), dr["PodCastRoot"].ToString(), false, null, bool.Parse(dr["DownloadAll"].ToString()), logger);
                        a.fk_genre = dr["FK_Genre"].ToString();
                        a.useDefault = bool.Parse(dr["useDefault"].ToString());
                        a.DefaultArtistName = dr["DefaultArtistName"].ToString();
                        a.id = Guid.Parse(dr[0].ToString());
                        resultset.Add(a);
                    }
                }
                if (!dr.IsClosed)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
            podcasts = resultset;
            return resultset;
        }

        public Data.Model.PodCast getByID(Guid id)
        {
            List<PodCast> podcastlist = null;
            if (podcasts == null)
            {
                podcastlist = getAll();
            }
            else
            {
                podcastlist = podcasts;
            }
            foreach (PodCast a in podcastlist)
            {
                if (a.id == id)
                {
                    return a;
                }
            }
            return null;
        }


        public Data.Model.PodCast getByName(string name)
        {
            List<PodCast> podcastlist = null;
            if (podcasts == null)
            {
                podcastlist = getAll();
            }
            else
            {
                podcastlist = podcasts;
            }
            foreach (PodCast a in podcastlist)
            {
                if (a.podcastpath.ToLower() == name.ToLower())
                {
                    return a;
                }
            }
            return null;
        }
    }
}
