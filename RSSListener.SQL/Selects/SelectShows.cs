using System;
using System.Collections.Generic;
using System.Linq;
using RSSListener.Data.Model;
using System.Data.SqlClient;
using System.Text;

namespace RSSListener.SQL.Selects
{
    class SelectShows : RSSListener.SQL.Interface.Selects<RSSListener.Data.Model.Show>
    {
        private SqlConnection conn = null;
        private List<RSSListener.Data.Model.Show> shows = null;
        private const string storedproc = "selectShows";
        private cLogger logger = null;
        private PodCast podcast = null;
        public SelectShows(SqlConnection connection, cLogger logger, PodCast p=null)
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
                this.podcast = p;
                this.logger = logger;
            }
        }
        public List<Data.Model.Show> getAll()
        {
            List<Show> resultset = new List<Show>();
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
                        Show show = new Show(dr["Title"].ToString(), null, "", dr["URL"].ToString(), null, dr["TrackList"].ToString(), dr["Path"].ToString(), logger, podcast);
                        show.id = Guid.Parse(dr["id"].ToString());
                        resultset.Add(show);
                    }
                }
                if (!dr.IsClosed)
                {
                    dr.Close();
                }
                
            }
            shows = resultset;
            return resultset;
        }

        public Data.Model.Show getByID(Guid id)
        {
            List<Show> showlist = null;
            if (shows == null)
            {
                showlist = getAll();
            }
            else
            {
                showlist = shows;
            }
            foreach (Show a in showlist)
            {
                if (a.id == id)
                {
                    return a;
                }
            }
            return null;
        }

        public Data.Model.Show getByName(string name)
        {
            List<Show> showlist = null;
            if (shows == null)
            {
                showlist = getAll();
            }
            else
            {
                showlist = shows;
            }
            foreach (Show a in showlist)
            {
                if (a.title.ToLower() == name.ToLower())
                {
                    return a;
                }
            }
            return null;
        }
    }
}
