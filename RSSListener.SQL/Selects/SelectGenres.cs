using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using RSSListener.Data.Model;
using System.Text;

namespace RSSListener.SQL.Selects
{
    class SelectGenres : RSSListener.SQL.Interface.Selects<RSSListener.Data.Model.Genre>
    {
        private SqlConnection conn = null;
        private List<Genre> genres = null;
        private cLogger logger = null;
        private const string storedproc = "SelectGenres";
        public SelectGenres(SqlConnection connection, cLogger logger)
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

        public List<Genre> getAll()
        {
            List<Genre> resultset = new List<Genre>();
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
                        Genre a = new Genre(dr[1].ToString());
                        a.id = Guid.Parse(dr[0].ToString());
                        resultset.Add(a);
                    }
                }
                if (!dr.IsClosed)
                {
                    dr.Close();
                }
            }
            genres = resultset;
            return resultset;
        }

        public Genre getByID(Guid id)
        {
            List<Genre> genrelist = null;
            if (genres == null)
            {
                genrelist = getAll();
            }
            else
            {
                genrelist = genres;
            }
            foreach (Genre a in genrelist)
            {
                if (a.id == id)
                {
                    return a;
                }
            }
            return null;
        }

        public Genre getByName(string name)
        {
            List<Genre> genrelist = null;
            if (genres == null)
            {
                genrelist = getAll();
            }
            else
            {
                genrelist = genres;
            }
            foreach (Genre a in genrelist)
            {
                if (a.GenreName.ToLower() == name.ToLower())
                {
                    return a;
                }
            }
            return null;
        }

    }
}
