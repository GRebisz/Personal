using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RSSListener.Data.Model;
using System.Data.SqlClient;

namespace RSSListener.SQL.Selects
{
    class SelectArtists : RSSListener.SQL.Interface.Selects<RSSListener.Data.Model.Artist>
    {
        private SqlConnection conn = null;
        private List<Artist> artists = null;
        private const string storedproc = "selectArtists";
        private cLogger logger = null;
        public SelectArtists(SqlConnection connection, cLogger logger)
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
                this.logger = logger;
            }
        }
        public List<Artist> getAll()
        {
            List<Artist> resultset = new List<Artist>();
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
                        Artist a = new Artist(dr[1].ToString(), bool.Parse(dr[2].ToString()));
                        a.id = Guid.Parse(dr[0].ToString());
                        resultset.Add(a);
                    }
                }
                if (!dr.IsClosed)
                {
                    dr.Close();
                }
            }
            artists = resultset;
            return resultset;
        }

        public Artist getByID(Guid id)
        {
            List<Artist> ArtistList = null;
            if (artists == null)
            {
                ArtistList = getAll();
            }
            else
            {
                ArtistList = artists;
            }
            foreach (Artist a in ArtistList)
            {
                if (a.id == id)
                {
                    return a;
                }
            }
            return null;
        }


        public Artist getByName(string name)
        {
            List<Artist> ArtistList = null;
            if (artists == null)
            {
                ArtistList = getAll();
            }
            else
            {
                ArtistList = artists;
            }
            foreach (Artist a in ArtistList)
            {
                if (a.Artistname.ToLower() == name.ToLower())
                {
                    return a;
                }
            }
            return null;
        }
    }
}
