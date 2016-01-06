using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
namespace RSSListener.SQL.Inserts
{
    public class InsertShow : RSSListener.SQL.Interface.Inserts
    {
        private SqlConnection conn = null;
        private RSSListener.Data.Model.Show show = null;
        private const string storedproc = "InsertShow";
        public InsertShow(SqlConnection connection, RSSListener.Data.Model.Show show)
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
                this.show = show;
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
                comm.Parameters.AddWithValue("@fk_podcast", show.parentpodcast.id);
                comm.Parameters.AddWithValue("@fk_artist", show.artist.id);
                comm.Parameters.AddWithValue("@title", show.title);
                comm.Parameters.AddWithValue("@url", show.url);
                comm.Parameters.AddWithValue("@path", show.dnbpath);
                comm.Parameters.AddWithValue("@recorded", show.Recorded);
                comm.Parameters.AddWithValue("@tracklist", show.Tracklist);
                comm.Parameters.AddWithValue("@fk_subgenre", null);
                comm.Parameters.AddWithValue("@filename", show.filename);
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
            if (show.artist == null)
            {
                throw new ArgumentNullException("show.artist");
            }
            else if (show.genre == null)
            {
                throw new ArgumentNullException("show.genre", "No Genre");
            }
            else if (String.IsNullOrWhiteSpace(show.dnbpath) || String.IsNullOrWhiteSpace(show.title) || String.IsNullOrWhiteSpace(show.url) ||
                String.IsNullOrWhiteSpace(show.dnbpath) || String.IsNullOrWhiteSpace(show.filename))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public string getblank(string input)
        {
            if (String.IsNullOrWhiteSpace(input))
            {
                return "";
            }
            return input;
        }
    }
}
