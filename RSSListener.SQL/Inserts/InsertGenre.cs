using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Text;

namespace RSSListener.SQL.Inserts
{
    public class InsertGenre : RSSListener.SQL.Interface.Inserts
    {
        private SqlConnection conn = null;
        public RSSListener.Data.Model.Genre genre = null;
        private const string storedproc = "InsertGenre";
        public InsertGenre(SqlConnection connection, RSSListener.Data.Model.Genre genre)
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
                this.genre = genre;
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
                comm.Parameters.Add(new SqlParameter("@GenreName", Capitalise(genre.GenreName)));
                SqlDataReader dr = comm.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    genre.id = Guid.Parse(dr[0].ToString());
                    dr.Close();
                    dr.Dispose();
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
            if (genre == null)
            {
                throw new ArgumentNullException("genre");
            }
            else
            {
                if (String.IsNullOrWhiteSpace(genre.GenreName))
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
