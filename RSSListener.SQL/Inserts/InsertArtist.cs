using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace RSSListener.SQL.Inserts
{
    public class InsertArtist : RSSListener.SQL.Interface.Inserts
    {
        private SqlConnection conn = null;
        public RSSListener.Data.Model.Artist artist = null;
        private const string storedproc = "InsertArtist";
        public InsertArtist(SqlConnection connection, RSSListener.Data.Model.Artist artist)
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
                this.artist = artist;
            }
        }
        public RSSListener.Data.Model.Artist getArtist()
        {
            return artist;
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
                comm.Parameters.Add(new SqlParameter("@ArtistName", Capitalise(artist.Artistname)));
                comm.Parameters.Add(new SqlParameter("@isdj", 1));
                SqlDataReader dr = comm.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    artist.id = Guid.Parse(dr[0].ToString());
                    dr.Close();
                    endres = true;
                }
            }
            else
            {
                endres = false;
            }
            return endres;
        }
        private string Capitalise(string input)
        {
            string[] spaced = input.Split(' ');
            string newstring = "";
            foreach (string s in spaced)
            {
                if (s.Length > 1)
                {
                    newstring += s[0].ToString().ToUpper() + s.Substring(1) + " ";
                }
                else
                {
                    newstring += s.ToString().ToUpper();
                }
            }
            return newstring;
        }
        public bool isValid()
        {
            if (artist == null)
            {
                throw new ArgumentNullException("artist");
            }
            else
            {
                if (String.IsNullOrWhiteSpace(artist.Artistname))
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
}
